using System.Collections.Concurrent;

namespace MDMPI.App.Api.WebSockets;

/// <summary>
/// Caches the latest normalized Location envelope per RequestID so a newly
/// connected (or reconnected) client gets the current picture immediately
/// instead of a blank map until the rider next moves 15 meters.
/// TTL plus max-entry cap, evicted lazily on write/read — no timers.
/// </summary>
internal sealed class LocationReplayCache
{
    private readonly ConcurrentDictionary<string, CacheEntry> _entries = new();
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _ttl;
    private readonly int _maxEntries;

    public LocationReplayCache(TimeProvider timeProvider, TimeSpan ttl, int maxEntries)
    {
        _timeProvider = timeProvider;
        _ttl = ttl;
        _maxEntries = maxEntries;
    }

    public int Count => _entries.Count;

    public void Store(string requestId, string normalizedJson)
    {
        var now = _timeProvider.GetUtcNow();
        _entries[requestId] = new CacheEntry(normalizedJson, now);

        EvictExpired(now);

        while (_entries.Count > _maxEntries)
        {
            // O(n) oldest scan is fine at the configured cap (hundreds of entries).
            var oldest = _entries.MinBy(pair => pair.Value.StoredAtUtc);
            if (oldest.Key is null || !_entries.TryRemove(oldest.Key, out _))
            {
                break;
            }
        }
    }

    public IReadOnlyList<string> GetLiveEnvelopes()
    {
        var now = _timeProvider.GetUtcNow();
        var live = new List<string>();

        foreach (var pair in _entries)
        {
            if (now - pair.Value.StoredAtUtc >= _ttl)
            {
                _entries.TryRemove(pair.Key, out _);
                continue;
            }

            live.Add(pair.Value.NormalizedJson);
        }

        return live;
    }

    private void EvictExpired(DateTimeOffset now)
    {
        foreach (var pair in _entries)
        {
            if (now - pair.Value.StoredAtUtc >= _ttl)
            {
                _entries.TryRemove(pair.Key, out _);
            }
        }
    }

    private sealed record CacheEntry(string NormalizedJson, DateTimeOffset StoredAtUtc);
}
