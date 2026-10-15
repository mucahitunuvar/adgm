using System.Collections.Concurrent;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;

// ADR-017 Decision 1. IMemoryCache has no built-in way to enumerate or prefix-match its own keys,
// so RemoveByPrefix needs this service to track its own key set. Every Set() registers a
// PostEvictionCallback so a key is dropped from that set the moment IMemoryCache evicts it for any
// reason (natural TTL expiry included, not just an explicit Remove) - otherwise the tracking set
// would grow unboundedly with expired entries, which matters most for IUserScopedCacheService's
// per-user keys.
//
// _keys' value is a per-Set generation token (a fresh object reference), not a plain marker: a
// PostEvictionCallback is scheduled asynchronously by IMemoryCache (never invoked inline - see
// Microsoft.Extensions.Caching.Memory's CacheEntry.InvokeEvictionCallbacks, which posts it to the
// ThreadPool), so an explicit Remove/RemoveByPrefix for key K can return, have K's GetOrCreateAsync
// immediately repopulate K with a brand new entry, and only THEN have the OLD entry's callback
// finally run. If that callback removed _keys[K] unconditionally, it would untrack the NEW live
// entry it was never registered for, and a later RemoveByPrefix would then find nothing to remove -
// serving the stale value indefinitely. Tying the callback to the exact token it was registered
// with (via ICollection<KeyValuePair<,>>.Remove's atomic compare-and-remove) makes it a no-op once
// superseded, so only the generation it was actually registered for is ever untracked.
public sealed class MemoryCacheService(IMemoryCache cache, IOptions<CacheSettings> options) : ICacheService
{
    private readonly ConcurrentDictionary<string, object> _keys = new();
    private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(options.Value.DefaultTtlMinutes);

    // Exposed only to prove the PostEvictionCallback wiring actually shrinks this set on natural
    // TTL expiry, not just on an explicit Remove/RemoveByPrefix (see MemoryCacheServiceTests).
    internal int TrackedKeyCount => _keys.Count;

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(key, out T? cached) && cached is not null)
        {
            return cached;
        }

        var value = await factory(cancellationToken);

        var generation = new object();
        var entryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? _defaultTtl,
        };
        entryOptions.RegisterPostEvictionCallback(OnEvicted, (Service: this, Generation: generation));

        cache.Set(key, value, entryOptions);

        // Unconditional overwrite (not TryAdd): this call just produced the current generation for
        // key, superseding whatever generation (if any) was tracked before.
        _keys[key] = generation;

        return value;
    }

    public void Remove(string key)
    {
        // Deliberately does not touch _keys itself: reading _keys only after cache.Remove returns
        // would reopen the same race this type exists to close - a concurrent GetOrCreateAsync could
        // Set a brand new entry for key in between, and this call would then read AND remove THAT
        // entry's generation, even though it never touched that entry. Only the removed entry's own
        // OnEvicted callback (registered at Set-time for exactly this generation) is in a position to
        // know which generation it is actually safe to untrack.
        cache.Remove(key);
    }

    public void RemoveByPrefix(string prefix)
    {
        foreach (var key in _keys.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).ToList())
        {
            Remove(key);
        }
    }

    // Static + state (rather than a captured closure) so no per-Set allocation beyond the generation
    // token itself is needed just to know which instance's key set, and which generation, to clean up.
    private static void OnEvicted(object key, object? value, EvictionReason reason, object? state)
    {
        var (service, generation) = ((MemoryCacheService Service, object Generation))state!;
        ((ICollection<KeyValuePair<string, object>>)service._keys).Remove(
            new KeyValuePair<string, object>((string)key, generation));
    }
}
