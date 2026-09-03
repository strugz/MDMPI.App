using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using MDMPI.App.Api.WebSockets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MDMPI.App.Tests.WebSockets;

public sealed class WebSocketEndpointIntegrationTests
{
    private const string ApiKey = "test-websocket-key";

    [Fact]
    public async Task Connect_WithValidApiKey_EstablishesWebSocket()
    {
        await using var factory = CreateFactory();
        var client = factory.Server.CreateWebSocketClient();
        using var socket = await client.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);

        Assert.Equal(WebSocketState.Open, socket.State);
    }

    [Theory]
    [InlineData("/api/ws")]
    [InlineData("/api/ws?apiKey=wrong-key")]
    public async Task Connect_WithMissingOrInvalidApiKey_IsRejected(string path)
    {
        await using var factory = CreateFactory();
        var client = factory.Server.CreateWebSocketClient();

        await Assert.ThrowsAnyAsync<Exception>(() =>
            client.ConnectAsync(WebSocketUri(path), CancellationToken.None));
    }

    [Fact]
    public async Task Send_DirectLocation_BroadcastsWrappedLocationToOtherClientsOnly()
    {
        await using var factory = CreateFactory();
        var senderClient = factory.Server.CreateWebSocketClient();
        var receiverClient = factory.Server.CreateWebSocketClient();
        using var sender = await senderClient.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);
        using var receiver = await receiverClient.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);

        await SendStringAsync(sender, """
        {
          "Type": "location_update",
          "RequestID": "REQ-100",
          "RiderId": "RIDER-1",
          "Latitude": 14.5995,
          "Longitude": 120.9842,
          "Timestamp": "2026-05-26T10:00:00Z",
          "Status": "en_route",
          "RiderInitial": "JB",
          "ETA": "10 mins",
          "Distance": "2 km",
          "Client": "MDMPI"
        }
        """);

        var received = await ReceiveStringAsync(receiver);
        using var document = JsonDocument.Parse(received);

        Assert.Equal("Location", document.RootElement.GetProperty("Message").GetString());
        Assert.Equal("REQ-100", document.RootElement.GetProperty("LocationUpdate").GetProperty("RequestID").GetString());
        Assert.Equal("RIDER-1", document.RootElement.GetProperty("LocationUpdate").GetProperty("RiderId").GetString());
        Assert.Null(await TryReceiveStringAsync(sender, TimeSpan.FromMilliseconds(200)));
    }

    [Fact]
    public async Task Send_Notification_BroadcastsTitleAndBody()
    {
        await using var factory = CreateFactory();
        var senderClient = factory.Server.CreateWebSocketClient();
        var receiverClient = factory.Server.CreateWebSocketClient();
        using var sender = await senderClient.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);
        using var receiver = await receiverClient.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);

        await SendStringAsync(sender, """
        {
          "Message": "Notification",
          "LocationUpdate": {},
          "NotificationUpdate": {
            "Title": "Delivery update",
            "Body": "Rider has arrived"
          }
        }
        """);

        var received = await ReceiveStringAsync(receiver);
        using var document = JsonDocument.Parse(received);

        Assert.Equal("Notification", document.RootElement.GetProperty("Message").GetString());
        Assert.Equal("Delivery update", document.RootElement.GetProperty("NotificationUpdate").GetProperty("Title").GetString());
        Assert.Equal("Rider has arrived", document.RootElement.GetProperty("NotificationUpdate").GetProperty("Body").GetString());
    }

    [Fact]
    public async Task Send_InvalidJson_KeepsConnectionUsable()
    {
        await using var factory = CreateFactory();
        var senderClient = factory.Server.CreateWebSocketClient();
        var receiverClient = factory.Server.CreateWebSocketClient();
        using var sender = await senderClient.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);
        using var receiver = await receiverClient.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);

        await SendStringAsync(sender, "{ invalid json");
        await SendStringAsync(sender, """{"Type":"location_update","RequestID":"REQ-AFTER-BAD-JSON"}""");

        var received = await ReceiveStringAsync(receiver);
        using var document = JsonDocument.Parse(received);

        Assert.Equal("REQ-AFTER-BAD-JSON", document.RootElement.GetProperty("LocationUpdate").GetProperty("RequestID").GetString());
    }

    [Fact]
    public async Task Broadcast_AfterClientDisconnect_StillReachesRemainingClients()
    {
        await using var factory = CreateFactory();
        var senderClient = factory.Server.CreateWebSocketClient();
        var staleClient = factory.Server.CreateWebSocketClient();
        var receiverClient = factory.Server.CreateWebSocketClient();
        using var sender = await senderClient.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);
        using var stale = await staleClient.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);
        using var receiver = await receiverClient.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);

        await stale.CloseAsync(WebSocketCloseStatus.NormalClosure, "test disconnect", CancellationToken.None);
        await SendStringAsync(sender, """{"Type":"location_update","RequestID":"REQ-STILL-DELIVERED"}""");

        var received = await ReceiveStringAsync(receiver);
        using var document = JsonDocument.Parse(received);

        Assert.Equal("REQ-STILL-DELIVERED", document.RootElement.GetProperty("LocationUpdate").GetProperty("RequestID").GetString());
    }

    [Fact]
    public async Task Broadcast_AfterClientAbortsAbruptly_SenderStaysConnectedAndReceiverGetsMessage()
    {
        // Unlike the graceful-close test above, Abort() simulates a mobile client
        // dropping off cellular: no close handshake, socket just dies.
        await using var factory = CreateFactory();
        var senderClient = factory.Server.CreateWebSocketClient();
        var staleClient = factory.Server.CreateWebSocketClient();
        var receiverClient = factory.Server.CreateWebSocketClient();
        using var sender = await senderClient.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);
        using var stale = await staleClient.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);
        using var receiver = await receiverClient.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);

        stale.Abort();
        await SendStringAsync(sender, """{"Type":"location_update","RequestID":"REQ-AFTER-ABORT"}""");

        var received = await ReceiveStringAsync(receiver);
        using var document = JsonDocument.Parse(received);

        Assert.Equal("REQ-AFTER-ABORT", document.RootElement.GetProperty("LocationUpdate").GetProperty("RequestID").GetString());
        Assert.Equal(WebSocketState.Open, sender.State);
    }

    [Fact]
    public async Task Send_OversizedMessage_ClosesConnectionWithMessageTooBig()
    {
        await using var factory = CreateFactory();
        var client = factory.Server.CreateWebSocketClient();
        using var socket = await client.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);

        var oversized = "{\"Type\":\"location_update\",\"RequestID\":\"" + new string('x', 70_000) + "\"}";
        await SendStringAsync(socket, oversized);

        var buffer = new byte[4096];
        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        Assert.Equal(WebSocketMessageType.Close, result.MessageType);
        Assert.Equal(WebSocketCloseStatus.MessageTooBig, result.CloseStatus);
        await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
    }

    [Fact]
    public async Task Connect_WithClientIdAndRole_IdentityStoredOnConnection()
    {
        await using var factory = CreateFactory();
        var client = factory.Server.CreateWebSocketClient();
        using var socket = await client.ConnectAsync(
            WebSocketUri($"/api/ws?apiKey={ApiKey}&clientId=phone-1&role=rider"), CancellationToken.None);

        var handler = factory.Services.GetRequiredService<WebSocketConnectionHandler>();
        var snapshot = await WaitForClientAsync(handler, c => c.ClientId == "phone-1" && c.Role == "rider");

        Assert.True(snapshot, "Expected a tracked client with ClientId 'phone-1' and Role 'rider'.");
    }

    [Fact]
    public async Task Connect_WithoutIdentity_DefaultsToAnon()
    {
        await using var factory = CreateFactory();
        var client = factory.Server.CreateWebSocketClient();
        using var socket = await client.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);

        var handler = factory.Services.GetRequiredService<WebSocketConnectionHandler>();
        var found = await WaitForClientAsync(handler, c => c.ClientId == "anon" && c.Role == "unspecified");

        Assert.True(found, "Legacy clients without identity params must still connect as anon/unspecified.");
    }

    [Fact]
    public async Task Connect_WithMaliciousClientId_Sanitized()
    {
        await using var factory = CreateFactory();
        var client = factory.Server.CreateWebSocketClient();
        var malicious = Uri.EscapeDataString("bad\nid\"—injected");
        using var socket = await client.ConnectAsync(
            WebSocketUri($"/api/ws?apiKey={ApiKey}&clientId={malicious}&role=admin"), CancellationToken.None);

        var handler = factory.Services.GetRequiredService<WebSocketConnectionHandler>();
        var found = await WaitForClientAsync(handler, c => c.ClientId == "anon" && c.Role == "unspecified");

        Assert.True(found, "A clientId with control characters must fall back to anon before reaching logs.");
    }

    [Fact]
    public async Task Connect_AfterLocationBroadcast_ReceivesCachedLocationReplay()
    {
        await using var factory = CreateFactory();
        var senderClient = factory.Server.CreateWebSocketClient();
        var witnessClient = factory.Server.CreateWebSocketClient();
        using var sender = await senderClient.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);
        using var witness = await witnessClient.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);

        await SendStringAsync(sender, """{"Type":"location_update","RequestID":"REQ-CACHED","Latitude":14.5,"Longitude":121.0}""");
        // The witness receiving the broadcast proves the server has processed and
        // cached the message — only then is a late joiner's replay deterministic.
        await ReceiveStringAsync(witness);

        var lateJoinerClient = factory.Server.CreateWebSocketClient();
        using var lateJoiner = await lateJoinerClient.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);

        var replayed = await ReceiveStringAsync(lateJoiner);
        using var document = JsonDocument.Parse(replayed);

        Assert.Equal("Location", document.RootElement.GetProperty("Message").GetString());
        Assert.Equal("REQ-CACHED", document.RootElement.GetProperty("LocationUpdate").GetProperty("RequestID").GetString());
    }

    [Fact]
    public async Task Connect_WhenCacheEmpty_NoReplayFramesAndBroadcastStillWorks()
    {
        await using var factory = CreateFactory();
        var receiverClient = factory.Server.CreateWebSocketClient();
        using var receiver = await receiverClient.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);

        var phantom = await TryReceiveStringAsync(receiver, TimeSpan.FromMilliseconds(400));
        Assert.Null(phantom);

        var senderClient = factory.Server.CreateWebSocketClient();
        using var sender = await senderClient.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);
        await SendStringAsync(sender, """{"Type":"location_update","RequestID":"REQ-FRESH"}""");

        var received = await ReceiveStringAsync(receiver);
        using var document = JsonDocument.Parse(received);
        Assert.Equal("REQ-FRESH", document.RootElement.GetProperty("LocationUpdate").GetProperty("RequestID").GetString());
    }

    [Fact]
    public async Task Replay_SendsLatestLocationPerRequestIdOnly()
    {
        await using var factory = CreateFactory();
        var senderClient = factory.Server.CreateWebSocketClient();
        var witnessClient = factory.Server.CreateWebSocketClient();
        using var sender = await senderClient.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);
        using var witness = await witnessClient.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);

        await SendStringAsync(sender, """{"Type":"location_update","RequestID":"REQ-9","Latitude":10.0}""");
        await ReceiveStringAsync(witness);
        await SendStringAsync(sender, """{"Type":"location_update","RequestID":"REQ-9","Latitude":20.0}""");
        await ReceiveStringAsync(witness);

        var lateJoinerClient = factory.Server.CreateWebSocketClient();
        using var lateJoiner = await lateJoinerClient.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);

        var replayed = await ReceiveStringAsync(lateJoiner);
        using var document = JsonDocument.Parse(replayed);
        Assert.Equal(20.0, document.RootElement.GetProperty("LocationUpdate").GetProperty("Latitude").GetDouble());

        var extra = await TryReceiveStringAsync(lateJoiner, TimeSpan.FromMilliseconds(400));
        Assert.Null(extra);
    }

    [Fact]
    public async Task Connect_WhenAtMaxConnections_RejectedThenSlotFreedAfterDisconnect()
    {
        await using var factory = CreateFactory(new Dictionary<string, string?>
        {
            ["WebSocket:MaxConnections"] = "1"
        });
        var firstClient = factory.Server.CreateWebSocketClient();
        using var first = await firstClient.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);

        var secondClient = factory.Server.CreateWebSocketClient();
        await Assert.ThrowsAnyAsync<Exception>(() =>
            secondClient.ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None));

        await first.CloseAsync(WebSocketCloseStatus.NormalClosure, "freeing the slot", CancellationToken.None);

        // Server-side cleanup is asynchronous after the close handshake; poll briefly.
        WebSocket? third = null;
        for (var attempt = 0; attempt < 40 && third is null; attempt++)
        {
            try
            {
                third = await factory.Server.CreateWebSocketClient()
                    .ConnectAsync(WebSocketUri($"/api/ws?apiKey={ApiKey}"), CancellationToken.None);
            }
            catch
            {
                await Task.Delay(100);
            }
        }

        Assert.NotNull(third);
        third.Dispose();
    }

    private static async Task<bool> WaitForClientAsync(
        WebSocketConnectionHandler handler,
        Func<(string ConnectionId, string ClientId, string Role), bool> predicate)
    {
        // Registration happens just after the handshake response; poll briefly.
        for (var attempt = 0; attempt < 40; attempt++)
        {
            if (handler.SnapshotClientsForTesting().Any(predicate))
            {
                return true;
            }

            await Task.Delay(50);
        }

        return false;
    }

    private static WebApplicationFactory<Program> CreateFactory(Dictionary<string, string?>? extraSettings = null)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    var settings = new Dictionary<string, string?>
                    {
                        ["ALLOW_PRODUCTION_DB"] = "true",
                        ["WebSocket:ApiKey"] = ApiKey
                    };

                    if (extraSettings is not null)
                    {
                        foreach (var pair in extraSettings)
                        {
                            settings[pair.Key] = pair.Value;
                        }
                    }

                    configuration.AddInMemoryCollection(settings);
                });
            });
    }

    private static Uri WebSocketUri(string path)
    {
        return new Uri($"ws://localhost{path}");
    }

    private static async Task SendStringAsync(WebSocket socket, string message)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private static async Task<string> ReceiveStringAsync(WebSocket socket)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var result = await TryReceiveStringAsync(socket, timeout.Token);

        Assert.False(result.TimedOut, "Timed out waiting for WebSocket message.");
        return result.Message!;
    }

    private static async Task<string?> TryReceiveStringAsync(WebSocket socket, TimeSpan timeout)
    {
        using var cancellation = new CancellationTokenSource(timeout);
        var result = await TryReceiveStringAsync(socket, cancellation.Token);
        return result.TimedOut ? null : result.Message;
    }

    private static async Task<(bool TimedOut, string? Message)> TryReceiveStringAsync(WebSocket socket, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        using var messageStream = new MemoryStream();

        try
        {
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                messageStream.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);
        }
        catch (OperationCanceledException)
        {
            return (true, null);
        }

        return (false, Encoding.UTF8.GetString(messageStream.ToArray()));
    }
}
