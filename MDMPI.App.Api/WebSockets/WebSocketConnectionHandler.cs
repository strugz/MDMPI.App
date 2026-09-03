using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace MDMPI.App.Api.WebSockets;

public sealed class WebSocketConnectionHandler
{
    // Location/notification payloads are well under 4 KB; anything near this limit
    // is a misbehaving client. Prevents unbounded MemoryStream growth per message.
    private const int MaxMessageBytes = 64 * 1024;

    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromSeconds(1);

    private readonly ConcurrentDictionary<string, ConnectedClient> _clients = new();
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebSocketConnectionHandler> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly LocationReplayCache _locationCache;
    private readonly int _maxConnections;
    private readonly int _maxMessagesPerSecond;
    private readonly int _rateLimitStrikeWindows;

    public WebSocketConnectionHandler(
        IConfiguration configuration,
        ILogger<WebSocketConnectionHandler> logger,
        TimeProvider timeProvider)
    {
        _configuration = configuration;
        _logger = logger;
        _timeProvider = timeProvider;
        _maxConnections = Math.Max(1, configuration.GetValue("WebSocket:MaxConnections", 500));
        _maxMessagesPerSecond = Math.Max(1, configuration.GetValue("WebSocket:MaxMessagesPerSecond", 20));
        _rateLimitStrikeWindows = Math.Max(1, configuration.GetValue("WebSocket:RateLimitStrikeWindows", 3));
        _locationCache = new LocationReplayCache(
            timeProvider,
            TimeSpan.FromMinutes(Math.Max(1, configuration.GetValue("WebSocket:LocationCacheTtlMinutes", 30))),
            Math.Max(1, configuration.GetValue("WebSocket:LocationCacheMaxEntries", 500)));
    }

    // A broadcast send that cannot complete within this window means the client has
    // stopped reading (dead radio link, full TCP window). Drop it rather than letting
    // it stall the sender's receive loop. Internal so tests can shorten it.
    internal TimeSpan SendTimeout { get; set; } = TimeSpan.FromSeconds(5);

    public async Task HandleAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var configuredApiKey = _configuration["WebSocket:ApiKey"];
        var suppliedApiKey = context.Request.Query["apiKey"].ToString();

        if (string.IsNullOrWhiteSpace(configuredApiKey) || !string.Equals(suppliedApiKey, configuredApiKey, StringComparison.Ordinal))
        {
            _logger.LogWarning("Rejected WebSocket connection from {RemoteIp}: invalid API key.", context.Connection.RemoteIpAddress);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        // Soft cap: a burst of simultaneous handshakes can exceed it by one or two,
        // which is harmless — the goal is bounding resources, not exact admission.
        if (_clients.Count >= _maxConnections)
        {
            _logger.LogWarning(
                "Rejected WebSocket connection from {RemoteIp}: connection limit of {MaxConnections} reached.",
                context.Connection.RemoteIpAddress,
                _maxConnections);
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return;
        }

        // Identity is claimed by the client, not proven; sanitize before it can
        // reach a log line (CR/LF forging, oversized garbage).
        var clientId = SanitizeIdentity(context.Request.Query["clientId"].ToString(), "anon");
        var role = NormalizeRole(context.Request.Query["role"].ToString());

        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        var connectionId = Guid.NewGuid().ToString("N");
        var client = new ConnectedClient(connectionId, socket, clientId, role, _timeProvider.GetUtcNow());

        _clients[connectionId] = client;
        _logger.LogInformation(
            "WebSocket client connected: {ConnectionId} ({ClientId}/{Role}). Active clients: {ClientCount}.",
            connectionId, clientId, role, _clients.Count);

        try
        {
            await ReplayCachedLocationsAsync(client);
            await ReceiveMessagesAsync(client, context.RequestAborted);
        }
        catch (Exception ex) when (ex is WebSocketException or OperationCanceledException)
        {
            // Mobile clients drop off cellular abruptly all the time; a reset mid-receive
            // is normal lifecycle, not a server error.
            _logger.LogDebug(ex, "WebSocket client {ConnectionId} ({ClientId}) disconnected abruptly.", connectionId, clientId);
        }
        finally
        {
            _clients.TryRemove(connectionId, out _);
            client.Dispose();
            _logger.LogInformation(
                "WebSocket client disconnected: {ConnectionId} ({ClientId}/{Role}). Active clients: {ClientCount}.",
                connectionId, clientId, role, _clients.Count);
        }
    }

    // A reconnecting phone should never stare at a blank map waiting for the rider's
    // next 15-meter movement: push the last known location of every active delivery.
    private async Task ReplayCachedLocationsAsync(ConnectedClient client)
    {
        foreach (var envelope in _locationCache.GetLiveEnvelopes())
        {
            if (client.Socket.State != WebSocketState.Open)
            {
                return;
            }

            await SendToClientAsync(client, Encoding.UTF8.GetBytes(envelope));
        }
    }

    private async Task ReceiveMessagesAsync(ConnectedClient client, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];

        while (!cancellationToken.IsCancellationRequested && client.Socket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result;
            await using var messageStream = new MemoryStream();

            do
            {
                result = await client.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await CloseSocketAsync(client.Socket, result.CloseStatus, result.CloseStatusDescription, cancellationToken);
                    return;
                }

                messageStream.Write(buffer, 0, result.Count);

                if (messageStream.Length > MaxMessageBytes)
                {
                    _logger.LogWarning(
                        "WebSocket client {ConnectionId} ({ClientId}) exceeded the {MaxMessageBytes}-byte message limit; closing connection.",
                        client.ConnectionId,
                        client.ClientId,
                        MaxMessageBytes);
                    await client.Socket.CloseAsync(WebSocketCloseStatus.MessageTooBig, "Message exceeds size limit.", cancellationToken);
                    return;
                }
            }
            while (!result.EndOfMessage);

            if (result.MessageType != WebSocketMessageType.Text)
            {
                continue;
            }

            // Rate limit before any parsing: malformed spam must consume budget too.
            if (IsRateLimited(client, out var closeConnection))
            {
                if (closeConnection)
                {
                    _logger.LogWarning(
                        "WebSocket client {ConnectionId} ({ClientId}) stayed over {MaxMessagesPerSecond} messages/second for {StrikeWindows} consecutive windows; closing connection.",
                        client.ConnectionId,
                        client.ClientId,
                        _maxMessagesPerSecond,
                        _rateLimitStrikeWindows);
                    await client.Socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Message rate limit exceeded.", cancellationToken);
                    return;
                }

                continue;
            }

            var message = Encoding.UTF8.GetString(messageStream.ToArray());
            var normalized = WebSocketMessageNormalizer.Normalize(message);

            if (normalized.InvalidJson)
            {
                _logger.LogWarning("Invalid JSON received from WebSocket client {ConnectionId} ({ClientId}).", client.ConnectionId, client.ClientId);
                continue;
            }

            if (!normalized.Success || normalized.NormalizedJson is null)
            {
                _logger.LogWarning("Unknown WebSocket message shape received from client {ConnectionId} ({ClientId}).", client.ConnectionId, client.ClientId);
                continue;
            }

            if (string.Equals(normalized.MessageType, "Location", StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(normalized.RequestID))
            {
                _locationCache.Store(normalized.RequestID, normalized.NormalizedJson);
            }

            await BroadcastAsync(client.ConnectionId, normalized.NormalizedJson);
        }
    }

    // Fixed one-second window. Normal cadence is one message per 15 m of movement,
    // so the default limit leaves enormous headroom; this exists to stop a runaway
    // client from multiplying its spam across every receiver via the fan-out.
    // No locking: each connection's counters are touched only by its own receive loop.
    private bool IsRateLimited(ConnectedClient client, out bool closeConnection)
    {
        closeConnection = false;
        var now = _timeProvider.GetUtcNow();

        if (now - client.WindowStart >= RateLimitWindow)
        {
            client.ConsecutiveStrikeWindows = client.MessagesInWindow > _maxMessagesPerSecond
                ? client.ConsecutiveStrikeWindows + 1
                : 0;
            client.WindowStart = now;
            client.MessagesInWindow = 0;
            client.WarnedThisWindow = false;
        }

        client.MessagesInWindow++;

        if (client.MessagesInWindow <= _maxMessagesPerSecond)
        {
            return false;
        }

        if (!client.WarnedThisWindow)
        {
            client.WarnedThisWindow = true;
            _logger.LogWarning(
                "WebSocket client {ConnectionId} ({ClientId}) exceeded {MaxMessagesPerSecond} messages/second; dropping excess messages.",
                client.ConnectionId,
                client.ClientId,
                _maxMessagesPerSecond);
        }

        // The current window is over the limit; if the previous windows were all
        // over the limit too, the abuse is sustained — close with a policy code.
        closeConnection = client.ConsecutiveStrikeWindows >= _rateLimitStrikeWindows - 1;
        return true;
    }

    // Internal so tests can drive broadcasts against fake sockets deterministically.
    internal async Task BroadcastAsync(string senderConnectionId, string message)
    {
        var payload = Encoding.UTF8.GetBytes(message);
        var tasks = _clients.Values
            .Where(client => client.ConnectionId != senderConnectionId && client.Socket.State == WebSocketState.Open)
            .Select(client => SendToClientAsync(client, payload));

        await Task.WhenAll(tasks);
    }

    private async Task SendToClientAsync(ConnectedClient client, byte[] payload)
    {
        try
        {
            using var timeout = new CancellationTokenSource(SendTimeout);

            await client.SendLock.WaitAsync(timeout.Token);
            try
            {
                await client.Socket.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Text, true, timeout.Token);
            }
            finally
            {
                client.SendLock.Release();
            }
        }
        catch (Exception ex) when (ex is WebSocketException or OperationCanceledException or ObjectDisposedException)
        {
            _logger.LogWarning(ex, "Failed to send WebSocket message to client {ConnectionId} ({ClientId}); dropping the connection.", client.ConnectionId, client.ClientId);
            _clients.TryRemove(client.ConnectionId, out _);

            // Do NOT dispose the client here: its own HandleAsync finally block owns
            // disposal, and disposing mid-broadcast releases a disposed SendLock,
            // faulting Task.WhenAll and tearing down the *sender's* connection.
            // Abort() wakes the dead client's receive loop so its cleanup runs.
            try
            {
                client.Socket.Abort();
            }
            catch (ObjectDisposedException)
            {
                // Already cleaned up by its receive loop; nothing to do.
            }
        }
    }

    internal static string SanitizeIdentity(string? raw, string fallback)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return fallback;
        }

        var value = raw.Trim();
        if (value.Length > 64)
        {
            return fallback;
        }

        foreach (var ch in value)
        {
            var allowed = ch is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or (>= '0' and <= '9')
                or '_' or '.' or ':' or '@' or '-';
            if (!allowed)
            {
                return fallback;
            }
        }

        return value;
    }

    internal static string NormalizeRole(string? raw)
    {
        return raw switch
        {
            "rider" => "rider",
            "watcher" => "watcher",
            _ => "unspecified"
        };
    }

    // Test hooks: register fake sockets and inspect state without an HTTP handshake.
    internal LocationReplayCache CacheForTesting => _locationCache;

    internal string TrackClientForTesting(WebSocket socket, string clientId = "anon", string role = "unspecified")
    {
        var connectionId = Guid.NewGuid().ToString("N");
        _clients[connectionId] = new ConnectedClient(connectionId, socket, clientId, role, _timeProvider.GetUtcNow());
        return connectionId;
    }

    internal bool IsClientTracked(string connectionId)
    {
        return _clients.ContainsKey(connectionId);
    }

    internal IReadOnlyList<(string ConnectionId, string ClientId, string Role)> SnapshotClientsForTesting()
    {
        return _clients.Values.Select(client => (client.ConnectionId, client.ClientId, client.Role)).ToList();
    }

    internal async Task RunReceiveLoopForTesting(WebSocket socket, CancellationToken cancellationToken)
    {
        var connectionId = TrackClientForTesting(socket);
        var client = _clients[connectionId];
        try
        {
            await ReceiveMessagesAsync(client, cancellationToken);
        }
        finally
        {
            _clients.TryRemove(connectionId, out _);
            client.Dispose();
        }
    }

    private static async Task CloseSocketAsync(
        WebSocket socket,
        WebSocketCloseStatus? closeStatus,
        string? closeStatusDescription,
        CancellationToken cancellationToken)
    {
        if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
        {
            await socket.CloseAsync(
                closeStatus ?? WebSocketCloseStatus.NormalClosure,
                closeStatusDescription,
                cancellationToken);
        }
    }

    private sealed class ConnectedClient : IDisposable
    {
        public ConnectedClient(string connectionId, WebSocket socket, string clientId, string role, DateTimeOffset windowStart)
        {
            ConnectionId = connectionId;
            Socket = socket;
            ClientId = clientId;
            Role = role;
            WindowStart = windowStart;
        }

        public string ConnectionId { get; }

        public WebSocket Socket { get; }

        public string ClientId { get; }

        public string Role { get; }

        public SemaphoreSlim SendLock { get; } = new(1, 1);

        // Rate-limit state: read and written only by this connection's receive loop.
        public DateTimeOffset WindowStart { get; set; }

        public int MessagesInWindow { get; set; }

        public int ConsecutiveStrikeWindows { get; set; }

        public bool WarnedThisWindow { get; set; }

        public void Dispose()
        {
            SendLock.Dispose();
        }
    }
}
