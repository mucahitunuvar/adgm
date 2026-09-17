using GenclikMerkezi.SharedKernel.Abstractions;

namespace GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;

// ADR-017 Decision 2.
public sealed class UserScopedCacheService(ICacheService cacheService, ICurrentUserContext currentUserContext)
    : IUserScopedCacheService
{
    public Task<T> GetOrCreateForCurrentUserAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default)
    {
        // No current user (anonymous/unauthenticated caller): never cache under a shared key one
        // anonymous caller's result could otherwise leak into another's. Just run the factory.
        if (currentUserContext.UserId is not { } userId)
        {
            return factory(cancellationToken);
        }

        var scopedKey = $"user:{userId}:{key}";
        return cacheService.GetOrCreateAsync(scopedKey, factory, ttl, cancellationToken);
    }
}
