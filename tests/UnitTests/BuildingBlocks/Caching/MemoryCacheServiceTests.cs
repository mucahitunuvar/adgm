using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.BuildingBlocks.Caching;

public class MemoryCacheServiceTests
{
    private static MemoryCacheService CreateService(int defaultTtlMinutes = 60) =>
        new(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheSettings { DefaultTtlMinutes = defaultTtlMinutes }));

    [Fact]
    public async Task GetOrCreateAsync_OnMiss_InvokesFactoryAndCachesResult()
    {
        var service = CreateService();
        var callCount = 0;

        var result = await service.GetOrCreateAsync("key", _ =>
        {
            callCount++;
            return Task.FromResult("value");
        });

        Assert.Equal("value", result);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task GetOrCreateAsync_OnHit_DoesNotInvokeFactoryAgain()
    {
        var service = CreateService();
        var callCount = 0;

        Task<string> Factory(CancellationToken _)
        {
            callCount++;
            return Task.FromResult("value");
        }

        await service.GetOrCreateAsync("key", Factory);
        var second = await service.GetOrCreateAsync("key", Factory);

        Assert.Equal("value", second);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task Remove_ThenGetOrCreateAsync_InvokesFactoryAgain()
    {
        var service = CreateService();
        var callCount = 0;

        Task<string> Factory(CancellationToken _)
        {
            callCount++;
            return Task.FromResult($"value-{callCount}");
        }

        await service.GetOrCreateAsync("key", Factory);
        service.Remove("key");
        var afterRemove = await service.GetOrCreateAsync("key", Factory);

        Assert.Equal("value-2", afterRemove);
        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task RemoveByPrefix_RemovesOnlyMatchingKeys()
    {
        var service = CreateService();
        await service.GetOrCreateAsync("referencedata:list:Sector:true", _ => Task.FromResult("a"));
        await service.GetOrCreateAsync("referencedata:list:Sector:false", _ => Task.FromResult("b"));
        await service.GetOrCreateAsync("referencedata:list:Currency:true", _ => Task.FromResult("c"));

        service.RemoveByPrefix("referencedata:list:Sector:");

        var sectorCallCount = 0;
        await service.GetOrCreateAsync("referencedata:list:Sector:true", _ =>
        {
            sectorCallCount++;
            return Task.FromResult("a-again");
        });
        Assert.Equal(1, sectorCallCount);

        var currencyCallCount = 0;
        await service.GetOrCreateAsync("referencedata:list:Currency:true", _ =>
        {
            currencyCallCount++;
            return Task.FromResult("c-again");
        });
        Assert.Equal(0, currencyCallCount);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithNullTtl_UsesConfiguredDefaultTtl()
    {
        // A custom (non-default) DefaultTtlMinutes, to prove GetOrCreateAsync actually reads
        // CacheSettings.DefaultTtlMinutes for a null ttl rather than a hardcoded constant - if it
        // ignored the option, this would behave identically to the 60-minute default tests above,
        // which would not catch a bug here.
        var service = CreateService(defaultTtlMinutes: 45);

        var result = await service.GetOrCreateAsync("key", _ => Task.FromResult("value"));
        var cachedResult = await service.GetOrCreateAsync("key", _ => Task.FromResult("should-not-be-called"));

        Assert.Equal("value", result);
        Assert.Equal("value", cachedResult);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithExplicitTtl_OverridesConfiguredDefault()
    {
        var service = CreateService(defaultTtlMinutes: 60);

        await service.GetOrCreateAsync("key", _ => Task.FromResult("value"), TimeSpan.FromMilliseconds(20));
        await Task.Delay(100);

        var callCount = 0;
        await service.GetOrCreateAsync("key", _ =>
        {
            callCount++;
            return Task.FromResult("value-again");
        });

        // If the explicit short ttl had been ignored in favor of the 60-minute default, this would
        // still be a cache hit (callCount == 0).
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenEntryNaturallyExpires_RemovesItFromTrackedKeySet()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new MemoryCacheService(memoryCache, Options.Create(new CacheSettings { DefaultTtlMinutes = 60 }));

        await service.GetOrCreateAsync("expiring-key", _ => Task.FromResult("value"), TimeSpan.FromMilliseconds(20));
        await service.GetOrCreateAsync("kept-key", _ => Task.FromResult("value"), TimeSpan.FromMinutes(10));
        Assert.Equal(2, service.TrackedKeyCount);

        await Task.Delay(100);

        // Reading the underlying IMemoryCache directly (not through MemoryCacheService, which would
        // immediately re-add the key on a miss and mask the intermediate state) forces it to notice
        // "expiring-key" is past its absolute expiration and evict it, firing
        // MemoryCacheService's PostEvictionCallback.
        memoryCache.TryGetValue("expiring-key", out object? _);

        // IMemoryCache does not guarantee the eviction callback runs synchronously within
        // TryGetValue, so this polls briefly instead of asserting immediately.
        var deadline = DateTime.UtcNow.AddSeconds(2);
        while (service.TrackedKeyCount > 1 && DateTime.UtcNow < deadline)
        {
            await Task.Delay(10);
        }

        Assert.Equal(1, service.TrackedKeyCount);
    }
}
