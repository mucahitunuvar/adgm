namespace GenclikMerkezi.SharedKernel.Abstractions;

// The standard caching contract (ADR-017) - global/module-level cache. For a cache keyed by the
// current caller (e.g. "this candidate's own recommendations"), use IUserScopedCacheService
// instead. Implemented against IMemoryCache today (MemoryCacheService, BuildingBlocks.Infrastructure);
// if distributed caching (Redis) becomes necessary, only that implementation changes.
public interface ICacheService
{
    // Returns the cached value for key if present; otherwise invokes factory, caches its result for
    // ttl (or the service's configured default when ttl is null), and returns it.
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default);

    void Remove(string key);

    // Removes every currently cached key that starts with prefix - the mechanism cache
    // invalidation after a mutation uses when multiple keys (e.g. one per page/filter combination)
    // were derived from the same underlying data.
    void RemoveByPrefix(string prefix);
}
