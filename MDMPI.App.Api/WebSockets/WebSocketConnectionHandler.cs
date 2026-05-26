using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace MDMPI.App.Api.WebSockets;

public sealed class WebSocketConnectionHandler
{
    private readonly ConcurrentDictionary<string, ConnectedClient> _clients = new();
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebSocketConnectionHandler> _logger;

    public WebSocketConnectionHandler(IConfiguration configuration, ILogger<WebSocketConnectionHandler> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

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

        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        var connectionId = Guid.NewGuid().ToString("N");
        var client = new ConnectedClient(connectionId, socket);

        _clients[connectionId] = client;
        _logger.LogInformation("WebSocket client connected: {ConnectionId}. Active clients: {ClientCount}.", connectionId, _clients.Count);

        try
        {
            await ReceiveMessagesAsync(client, context.RequestAborted);
        }
        finally
        {
            _clients.TryRemove(connectionId, out _);
            client.Dispose();
            _logger.LogInformation("WebSocket client disconnected: {ConnectionId}. Active clients: {ClientCount}.", connectionId, _clients.Count);
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
            }
            while (!result.EndOfMessage);

            if (result.MessageType != WebSocketMessageType.Text)
            {
                continue;
            }

            var message = Encoding.UTF8.GetString(messageStream.ToArray());
            var normalized = WebSocketMessageNormalizer.Normalize(message);

            if (normalized.InvalidJson)
            {
                _logger.LogWarning("Invalid JSON received from WebSocket client {ConnectionId}.", client.ConnectionId);
                continue;
            }

            if (!normalized.Success || normalized.NormalizedJson is null)
            {
                _logger.LogWarning("Unknown WebSocket message shape received from client {ConnectionId}.", client.ConnectionId);
                continue;
            }

            await BroadcastAsync(client.ConnectionId, normalized.NormalizedJson);
        }
    }

    private async Task BroadcastAsync(string senderConnectionId, string message)
    {
        var payload = Encoding.UTF8.GetBytes(message);
        var tasks = _clients.Values
            .Where(client => client.ConnectionId != senderConnectionId && client.Socket.State == WebSocketState.Open)
            .Select(client => SendToClientAsync(client, payload));

        await Task.WhenAll(tasks);
    }

    private async Task SendToClientAsync(ConnectedClient client, byte[] payload)
    {
        var lockTaken = false;
        try
        {
            await client.SendLock.WaitAsync();
            lockTaken = true;

            await client.Socket.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception ex) when (ex is WebSocketException or OperationCanceledException or ObjectDisposedException)
        {
            _logger.LogWarning(ex, "Failed to broadcast WebSocket message to client {ConnectionId}.", client.ConnectionId);
            _clients.TryRemove(client.ConnectionId, out _);
            client.Dispose();
        }
        finally
        {
            if (lockTaken)
            {
                client.SendLock.Release();
            }
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
        public ConnectedClient(string connectionId, WebSocket socket)
        {
            ConnectionId = connectionId;
            Socket = socket;
        }

        public string ConnectionId { get; }

        public WebSocket Socket { get; }

        public SemaphoreSlim SendLock { get; } = new(1, 1);

        public void Dispose()
        {
            SendLock.Dispose();
        }
    }
}
