using System.Net.WebSockets;
using System.Text;
using MDMPI.App.Api.WebSockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace MDMPI.App.Tests.WebSockets;

/// <summary>
/// Deterministic rate-limit tests: a ScriptedWebSocket feeds queued frames to the
/// real receive loop (via the internal RunReceiveLoopForTesting hook), and a
/// FakeTimeProvider rolls the one-second window between frames — no wall-clock
/// timing, no flakiness.
/// </summary>
public sealed class WebSocketRateLimitTests
{
    private const string LocationMessage = """{"Type":"location_update","RequestID":"REQ-RL"}""";

    private static WebSocketConnectionHandler CreateHandler(
        FakeTimeProvider time,
        int maxPerSecond,
        int strikeWindows = 3,
        ILogger<WebSocketConnectionHandler>? logger = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["WebSocket:MaxMessagesPerSecond"] = maxPerSecond.ToString(),
                ["WebSocket:RateLimitStrikeWindows"] = strikeWindows.ToString()
            })
            .Build();

        return new WebSocketConnectionHandler(configuration, logger ?? NullLogger<WebSocketConnectionHandler>.Instance, time);
    }

    [Fact]
    public async Task Receive_UnderLimit_AllMessagesBroadcast()
    {
        var time = new FakeTimeProvider();
        var handler = CreateHandler(time, maxPerSecond: 20);
        var receiver = new RecordingWebSocket();
        handler.TrackClientForTesting(receiver);

        var sender = new ScriptedWebSocket();
        sender.Enqueue(LocationMessage);
        sender.Enqueue(LocationMessage);
        sender.Enqueue(LocationMessage);

        await handler.RunReceiveLoopForTesting(sender, CancellationToken.None);

        Assert.Equal(3, receiver.SentPayloads.Count);
        Assert.Equal(WebSocketCloseStatus.NormalClosure, sender.RequestedCloseStatus);
    }

    [Fact]
    public async Task Receive_OverLimit_ExcessDroppedAndConnectionKept()
    {
        var time = new FakeTimeProvider();
        var handler = CreateHandler(time, maxPerSecond: 2);
        var receiver = new RecordingWebSocket();
        handler.TrackClientForTesting(receiver);

        var sender = new ScriptedWebSocket();
        for (var i = 0; i < 5; i++)
        {
            sender.Enqueue(LocationMessage);
        }

        await handler.RunReceiveLoopForTesting(sender, CancellationToken.None);

        Assert.Equal(2, receiver.SentPayloads.Count);
        Assert.Equal(WebSocketCloseStatus.NormalClosure, sender.RequestedCloseStatus);
    }

    [Fact]
    public async Task Receive_OverLimit_WarnsOncePerWindow()
    {
        var time = new FakeTimeProvider();
        var logger = new ListLogger();
        var handler = CreateHandler(time, maxPerSecond: 2, logger: logger);
        handler.TrackClientForTesting(new RecordingWebSocket());

        var sender = new ScriptedWebSocket();
        for (var i = 0; i < 6; i++)
        {
            sender.Enqueue(LocationMessage);
        }

        await handler.RunReceiveLoopForTesting(sender, CancellationToken.None);

        Assert.Equal(1, logger.Warnings.Count(w => w.Contains("messages/second")));
    }

    [Fact]
    public async Task Receive_SustainedAbuse_ClosesWith1008PolicyViolation()
    {
        var time = new FakeTimeProvider();
        var handler = CreateHandler(time, maxPerSecond: 2, strikeWindows: 3);
        var receiver = new RecordingWebSocket();
        handler.TrackClientForTesting(receiver);

        var sender = new ScriptedWebSocket();
        // Window 1: 3 messages (over the limit of 2).
        sender.Enqueue(LocationMessage);
        sender.Enqueue(LocationMessage);
        sender.Enqueue(LocationMessage);
        // Window 2: over again.
        sender.Enqueue(LocationMessage, () => time.Advance(TimeSpan.FromSeconds(1.1)));
        sender.Enqueue(LocationMessage);
        sender.Enqueue(LocationMessage);
        // Window 3: over a third consecutive time — must close.
        sender.Enqueue(LocationMessage, () => time.Advance(TimeSpan.FromSeconds(1.1)));
        sender.Enqueue(LocationMessage);
        sender.Enqueue(LocationMessage);

        await handler.RunReceiveLoopForTesting(sender, CancellationToken.None);

        Assert.Equal(WebSocketCloseStatus.PolicyViolation, sender.RequestedCloseStatus);
        Assert.Equal(6, receiver.SentPayloads.Count);
    }

    [Fact]
    public async Task Receive_AbuseThenCleanWindow_ResetsStrikes()
    {
        var time = new FakeTimeProvider();
        var handler = CreateHandler(time, maxPerSecond: 2, strikeWindows: 3);
        var receiver = new RecordingWebSocket();
        handler.TrackClientForTesting(receiver);

        var sender = new ScriptedWebSocket();
        // Window 1: over the limit.
        sender.Enqueue(LocationMessage);
        sender.Enqueue(LocationMessage);
        sender.Enqueue(LocationMessage);
        // Window 2: clean.
        sender.Enqueue(LocationMessage, () => time.Advance(TimeSpan.FromSeconds(1.1)));
        // Window 3: over again — but strikes were reset, so no close.
        sender.Enqueue(LocationMessage, () => time.Advance(TimeSpan.FromSeconds(1.1)));
        sender.Enqueue(LocationMessage);
        sender.Enqueue(LocationMessage);

        await handler.RunReceiveLoopForTesting(sender, CancellationToken.None);

        Assert.Equal(WebSocketCloseStatus.NormalClosure, sender.RequestedCloseStatus);
        Assert.Equal(5, receiver.SentPayloads.Count);
    }

    [Fact]
    public async Task Receive_InvalidJson_CountsTowardRateLimit()
    {
        var time = new FakeTimeProvider();
        var handler = CreateHandler(time, maxPerSecond: 2);
        var receiver = new RecordingWebSocket();
        handler.TrackClientForTesting(receiver);

        var sender = new ScriptedWebSocket();
        sender.Enqueue("{ not json");
        sender.Enqueue("{ also not json");
        sender.Enqueue(LocationMessage);

        await handler.RunReceiveLoopForTesting(sender, CancellationToken.None);

        // The two garbage frames consumed the whole window budget, so the valid
        // message was rate-dropped: malformed spam cannot bypass the limit.
        Assert.Empty(receiver.SentPayloads);
        Assert.Equal(WebSocketCloseStatus.NormalClosure, sender.RequestedCloseStatus);
    }

    private sealed class ScriptedWebSocket : WebSocket
    {
        private readonly Queue<(string Message, Action? BeforeDeliver)> _frames = new();
        private WebSocketState _state = WebSocketState.Open;

        public WebSocketCloseStatus? RequestedCloseStatus { get; private set; }

        public void Enqueue(string message, Action? beforeDeliver = null)
        {
            _frames.Enqueue((message, beforeDeliver));
        }

        public override WebSocketCloseStatus? CloseStatus => RequestedCloseStatus;

        public override string? CloseStatusDescription => null;

        public override WebSocketState State => _state;

        public override string? SubProtocol => null;

        public override void Abort()
        {
            _state = WebSocketState.Aborted;
        }

        public override Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
        {
            RequestedCloseStatus = closeStatus;
            _state = WebSocketState.Closed;
            return Task.CompletedTask;
        }

        public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
        {
            RequestedCloseStatus = closeStatus;
            _state = WebSocketState.Closed;
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
        }

        public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            if (_frames.Count == 0)
            {
                _state = WebSocketState.CloseReceived;
                return Task.FromResult(new WebSocketReceiveResult(0, WebSocketMessageType.Close, true, WebSocketCloseStatus.NormalClosure, "end of script"));
            }

            var (message, beforeDeliver) = _frames.Dequeue();
            beforeDeliver?.Invoke();

            var bytes = Encoding.UTF8.GetBytes(message);
            bytes.CopyTo(buffer.Array!, buffer.Offset);
            return Task.FromResult(new WebSocketReceiveResult(bytes.Length, WebSocketMessageType.Text, true));
        }

        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingWebSocket : WebSocket
    {
        public List<string> SentPayloads { get; } = new();

        public override WebSocketCloseStatus? CloseStatus => null;

        public override string? CloseStatusDescription => null;

        public override WebSocketState State => WebSocketState.Open;

        public override string? SubProtocol => null;

        public override void Abort()
        {
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
            throw new NotSupportedException("Rate-limit tests never receive on this socket.");
        }

        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            lock (SentPayloads)
            {
                SentPayloads.Add(Encoding.UTF8.GetString(buffer.Array!, buffer.Offset, buffer.Count));
            }

            return Task.CompletedTask;
        }
    }

    private sealed class ListLogger : ILogger<WebSocketConnectionHandler>
    {
        public List<string> Warnings { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Warning)
            {
                lock (Warnings)
                {
                    Warnings.Add(formatter(state, exception));
                }
            }
        }
    }
}
