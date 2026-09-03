using System.Net.WebSockets;
using System.Text;
using MDMPI.App.Api.WebSockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace MDMPI.App.Tests.WebSockets;

/// <summary>
/// Deterministic fault-injection tests for broadcast failure paths. The integration
/// tests can only reach these paths through timing races (a dead client must still be
/// in the connection dictionary when the broadcast fires), so fake sockets are
/// registered directly via the handler's internal test hooks.
/// </summary>
public sealed class WebSocketBroadcastFaultToleranceTests
{
    private static WebSocketConnectionHandler CreateHandler()
    {
        var configuration = new ConfigurationBuilder().Build();
        return new WebSocketConnectionHandler(configuration, NullLogger<WebSocketConnectionHandler>.Instance, TimeProvider.System);
    }

    [Fact]
    public async Task Broadcast_WhenOneClientFailsToSend_DoesNotThrowAndStillDeliversToHealthyClients()
    {
        // Regression: a failed send used to Dispose() the client's SemaphoreSlim and then
        // Release() it in a finally block, throwing ObjectDisposedException through
        // Task.WhenAll and tearing down the healthy sender's connection.
        var handler = CreateHandler();
        var healthy = new RecordingWebSocket();
        var faulty = new ThrowingWebSocket();
        var senderId = handler.TrackClientForTesting(new RecordingWebSocket());
        var healthyId = handler.TrackClientForTesting(healthy);
        var faultyId = handler.TrackClientForTesting(faulty);

        var exception = await Record.ExceptionAsync(() =>
            handler.BroadcastAsync(senderId, """{"Message":"Location"}"""));

        Assert.Null(exception);
        Assert.Single(healthy.SentPayloads);
        Assert.False(handler.IsClientTracked(faultyId));
        Assert.True(handler.IsClientTracked(healthyId));
        Assert.True(faulty.Aborted);
    }

    [Fact]
    public async Task Broadcast_WhenOneClientNeverCompletesSend_TimesOutAndDropsIt()
    {
        // Regression: sends used CancellationToken.None, so a client that stopped
        // reading (full TCP window) stalled the sender's receive loop indefinitely.
        var handler = CreateHandler();
        handler.SendTimeout = TimeSpan.FromMilliseconds(200);
        var healthy = new RecordingWebSocket();
        var hung = new HangingWebSocket();
        var senderId = handler.TrackClientForTesting(new RecordingWebSocket());
        var healthyId = handler.TrackClientForTesting(healthy);
        var hungId = handler.TrackClientForTesting(hung);

        var broadcast = handler.BroadcastAsync(senderId, """{"Message":"Location"}""");
        var completed = await Task.WhenAny(broadcast, Task.Delay(TimeSpan.FromSeconds(10)));

        Assert.Same(broadcast, completed);
        await broadcast;
        Assert.Single(healthy.SentPayloads);
        Assert.False(handler.IsClientTracked(hungId));
        Assert.True(handler.IsClientTracked(healthyId));
    }

    [Fact]
    public async Task Broadcast_WhenSendLockIsHeldIndefinitely_TimesOutAndDropsClient()
    {
        // The timeout must also cover waiting for the per-client send lock, not just
        // the send itself: two overlapping broadcasts to one hung client would
        // otherwise queue behind each other forever.
        var handler = CreateHandler();
        handler.SendTimeout = TimeSpan.FromMilliseconds(200);
        var hung = new HangingWebSocket();
        var senderId = handler.TrackClientForTesting(new RecordingWebSocket());
        var hungId = handler.TrackClientForTesting(hung);

        var first = handler.BroadcastAsync(senderId, """{"Message":"Location"}""");
        var second = handler.BroadcastAsync(senderId, """{"Message":"Location"}""");
        var both = Task.WhenAll(first, second);
        var completed = await Task.WhenAny(both, Task.Delay(TimeSpan.FromSeconds(10)));

        Assert.Same(both, completed);
        await both;
        Assert.False(handler.IsClientTracked(hungId));
    }

    private abstract class FakeWebSocket : WebSocket
    {
        public bool Aborted { get; private set; }

        public override WebSocketCloseStatus? CloseStatus => null;

        public override string? CloseStatusDescription => null;

        public override WebSocketState State => WebSocketState.Open;

        public override string? SubProtocol => null;

        public override void Abort()
        {
            Aborted = true;
        }

        public override Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
        }

        public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Broadcast tests never receive.");
        }
    }

    private sealed class RecordingWebSocket : FakeWebSocket
    {
        public List<string> SentPayloads { get; } = new();

        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            lock (SentPayloads)
            {
                SentPayloads.Add(Encoding.UTF8.GetString(buffer.Array!, buffer.Offset, buffer.Count));
            }

            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingWebSocket : FakeWebSocket
    {
        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            throw new WebSocketException(WebSocketError.ConnectionClosedPrematurely);
        }
    }

    private sealed class HangingWebSocket : FakeWebSocket
    {
        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            // Honors cancellation like a real socket, but never completes on its own.
            return Task.Delay(Timeout.Infinite, cancellationToken);
        }
    }
}
