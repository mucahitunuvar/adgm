namespace GenclikMerkezi.SharedKernel.Abstractions;

// User-scoped counterpart to ICacheService (ADR-017 Decision 2) - for data that must be cached
// separately per calling user (e.g. a candidate's own recommended jobs), rather than shared/global
// data (use ICacheService directly for that). The key each caller passes is combined with the
// current user's id; callers never build that combined key themselves.
public interface IUserScopedCacheService
{
    Task<T> GetOrCreateForCurrentUserAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default);
}
