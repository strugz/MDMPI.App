using MDMPI.App.Api.WebSockets;
using Microsoft.Extensions.Time.Testing;

namespace MDMPI.App.Tests.WebSockets;

public sealed class LocationReplayCacheTests
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(30);

    [Fact]
    public void Store_ThenGetLive_ReturnsEnvelope()
    {
        var time = new FakeTimeProvider();
        var cache = new LocationReplayCache(time, Ttl, maxEntries: 500);

        cache.Store("REQ-1", """{"Message":"Location"}""");

        var live = cache.GetLiveEnvelopes();
        Assert.Single(live);
        Assert.Equal("""{"Message":"Location"}""", live[0]);
    }

    [Fact]
    public void Store_SameRequestId_LastWriteWins()
    {
        var time = new FakeTimeProvider();
        var cache = new LocationReplayCache(time, Ttl, maxEntries: 500);

        cache.Store("REQ-1", "old");
        time.Advance(TimeSpan.FromMinutes(1));
        cache.Store("REQ-1", "new");

        var live = cache.GetLiveEnvelopes();
        Assert.Single(live);
        Assert.Equal("new", live[0]);
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void GetLive_AfterTtlExpiry_ReturnsEmptyAndEvicts()
    {
        var time = new FakeTimeProvider();
        var cache = new LocationReplayCache(time, Ttl, maxEntries: 500);

        cache.Store("REQ-1", "stale");
        time.Advance(TimeSpan.FromMinutes(31));

        Assert.Empty(cache.GetLiveEnvelopes());
        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public void Store_BeyondMaxEntries_EvictsOldest()
    {
        var time = new FakeTimeProvider();
        var cache = new LocationReplayCache(time, Ttl, maxEntries: 3);

        cache.Store("REQ-A", "a");
        time.Advance(TimeSpan.FromSeconds(1));
        cache.Store("REQ-B", "b");
        time.Advance(TimeSpan.FromSeconds(1));
        cache.Store("REQ-C", "c");
        time.Advance(TimeSpan.FromSeconds(1));
        cache.Store("REQ-D", "d");

        Assert.Equal(3, cache.Count);
        var live = cache.GetLiveEnvelopes();
        Assert.DoesNotContain("a", live);
        Assert.Contains("d", live);
    }

    [Fact]
    public void Store_EvictsExpiredBeforeCappingLiveEntries()
    {
        var time = new FakeTimeProvider();
        var cache = new LocationReplayCache(time, Ttl, maxEntries: 3);

        cache.Store("REQ-OLD1", "old1");
        cache.Store("REQ-OLD2", "old2");
        time.Advance(TimeSpan.FromMinutes(31));

        cache.Store("REQ-C", "c");
        cache.Store("REQ-D", "d");
        cache.Store("REQ-E", "e");

        Assert.Equal(3, cache.Count);
        var live = cache.GetLiveEnvelopes();
        Assert.Equal(new[] { "c", "d", "e" }, live.OrderBy(x => x));
    }
}
