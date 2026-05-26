using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

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

    private static WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["WebSocket:ApiKey"] = ApiKey
                    });
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
