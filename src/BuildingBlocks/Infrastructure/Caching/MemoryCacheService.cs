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
public sealed class MemoryCacheService(IMemoryCache cache, IOptions<CacheSettings> options) : ICacheService
{
    private readonly ConcurrentDictionary<string, byte> _keys = new();
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

        var entryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? _defaultTtl,
        };
        entryOptions.RegisterPostEvictionCallback(OnEvicted, this);

        cache.Set(key, value, entryOptions);
        _keys.TryAdd(key, 0);

        return value;
    }

    public void Remove(string key)
    {
        cache.Remove(key);
        _keys.TryRemove(key, out _);
    }

    public void RemoveByPrefix(string prefix)
    {
        foreach (var key in _keys.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).ToList())
        {
            Remove(key);
        }
    }

    // Static + state (rather than a captured closure) so no per-Set allocation is needed just to
    // know which MemoryCacheService instance's key set to clean up.
    private static void OnEvicted(object key, object? value, EvictionReason reason, object? state)
    {
        var service = (MemoryCacheService)state!;
        service._keys.TryRemove((string)key, out _);
    }
}
