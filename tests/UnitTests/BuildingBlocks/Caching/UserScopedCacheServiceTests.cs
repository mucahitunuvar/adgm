using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.BuildingBlocks.Caching;

public class UserScopedCacheServiceTests
{
    private static MemoryCacheService CreateCacheService() =>
        new(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings { DefaultTtlMinutes = 60 }));

    [Fact]
    public async Task GetOrCreateForCurrentUserAsync_OnHit_DoesNotInvokeFactoryAgain()
    {
        var service = new UserScopedCacheService(CreateCacheService(), new FakeCurrentUserContext(Guid.NewGuid()));
        var callCount = 0;

        Task<string> Factory(CancellationToken _)
        {
            callCount++;
            return Task.FromResult("value");
        }

        await service.GetOrCreateForCurrentUserAsync("key", Factory);
        var second = await service.GetOrCreateForCurrentUserAsync("key", Factory);

        Assert.Equal("value", second);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task GetOrCreateForCurrentUserAsync_DifferentUsers_SameKey_DoNotShareCacheEntries()
    {
        var cacheService = CreateCacheService();
        var userA = new UserScopedCacheService(cacheService, new FakeCurrentUserContext(Guid.NewGuid()));
        var userB = new UserScopedCacheService(cacheService, new FakeCurrentUserContext(Guid.NewGuid()));

        var resultA = await userA.GetOrCreateForCurrentUserAsync("recommendations", _ => Task.FromResult("A's data"));
        var resultB = await userB.GetOrCreateForCurrentUserAsync("recommendations", _ => Task.FromResult("B's data"));

        Assert.Equal("A's data", resultA);
        Assert.Equal("B's data", resultB);
    }

    [Fact]
    public async Task GetOrCreateForCurrentUserAsync_WithNoCurrentUser_NeverCaches()
    {
        var service = new UserScopedCacheService(CreateCacheService(), new FakeCurrentUserContext(userId: null));
        var callCount = 0;

        Task<string> Factory(CancellationToken _)
        {
            callCount++;
            return Task.FromResult($"value-{callCount}");
        }

        var first = await service.GetOrCreateForCurrentUserAsync("key", Factory);
        var second = await service.GetOrCreateForCurrentUserAsync("key", Factory);

        Assert.Equal("value-1", first);
        Assert.Equal("value-2", second);
        Assert.Equal(2, callCount);
    }
}
