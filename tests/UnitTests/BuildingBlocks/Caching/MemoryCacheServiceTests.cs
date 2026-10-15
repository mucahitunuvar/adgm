using System.Collections.Concurrent;
using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

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
    public async Task RemoveByPrefix_ThenImmediateRepopulate_SurvivesTheStaleEntrysDeferredEviction()
    {
        // Reproduces the suspected race deterministically (no wall-clock timing involved - a real
        // IMemoryCache's PostEvictionCallback fires asynchronously via Task.Factory.StartNew, and in a
        // warmed-up test process it usually wins the race against the very next line, which made a
        // Task.Delay-based reproduction unreliable in either direction). DeferredEvictionMemoryCache
        // behaves like a real IMemoryCache except it queues eviction callbacks instead of firing them,
        // so the test controls exactly when the stale entry's callback runs relative to the repopulate.
        var cache = new DeferredEvictionMemoryCache();
        var service = new MemoryCacheService(cache, Options.Create(new CacheSettings { DefaultTtlMinutes = 60 }));
        const string key = "referencedata:list:Sector:true";

        await service.GetOrCreateAsync(key, _ => Task.FromResult("v1"));
        service.RemoveByPrefix("referencedata:list:Sector:"); // queues v1's eviction callback, does not run it yet

        // Repopulate before the queued callback runs - the exact interleaving the hypothesis describes.
        await service.GetOrCreateAsync(key, _ => Task.FromResult("v2"));

        // Now let v1's eviction callback run, simulating the ThreadPool finally getting to it.
        cache.FlushPendingEvictions();

        service.RemoveByPrefix("referencedata:list:Sector:");

        var callCount = 0;
        var afterSecondInvalidate = await service.GetOrCreateAsync(key, _ =>
        {
            callCount++;
            return Task.FromResult("v3");
        });

        // If the stale (v1) entry's eviction callback had wrongly untracked "v2", RemoveByPrefix would
        // have been a no-op and this would still return the stale "v2" without invoking the factory.
        Assert.Equal(1, callCount);
        Assert.Equal("v3", afterSecondInvalidate);
    }

    // A minimal IMemoryCache test double that behaves like the real MemoryCache (CreateEntry + Dispose
    // commits the entry, Remove evicts it) except PostEvictionCallbacks are queued instead of invoked
    // immediately or on a background thread - giving the test full, deterministic control over when a
    // stale entry's callback runs relative to other operations, instead of racing the real ThreadPool.
    private sealed class DeferredEvictionMemoryCache : IMemoryCache
    {
        private readonly ConcurrentDictionary<object, FakeCacheEntry> _store = new();
        private readonly ConcurrentQueue<(FakeCacheEntry Entry, EvictionReason Reason)> _pendingEvictions = new();

        public ICacheEntry CreateEntry(object key) => new FakeCacheEntry(key, this);

        public bool TryGetValue(object key, out object? value)
        {
            if (_store.TryGetValue(key, out var entry))
            {
                value = entry.Value;
                return true;
            }

            value = null;
            return false;
        }

        public void Remove(object key)
        {
            if (_store.TryRemove(key, out var entry))
            {
                _pendingEvictions.Enqueue((entry, EvictionReason.Removed));
            }
        }

        public void FlushPendingEvictions()
        {
            while (_pendingEvictions.TryDequeue(out var pending))
            {
                foreach (var callback in pending.Entry.PostEvictionCallbacks)
                {
                    callback.EvictionCallback(pending.Entry.Key, pending.Entry.Value, pending.Reason, callback.State);
                }
            }
        }

        private void Commit(FakeCacheEntry entry) => _store[entry.Key] = entry;

        public void Dispose()
        {
        }

        private sealed class FakeCacheEntry(object key, DeferredEvictionMemoryCache owner) : ICacheEntry
        {
            public object Key { get; } = key;

            public object? Value { get; set; }

            public DateTimeOffset? AbsoluteExpiration { get; set; }

            public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

            public TimeSpan? SlidingExpiration { get; set; }

            public IList<IChangeToken> ExpirationTokens { get; } = new List<IChangeToken>();

            public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; } = new List<PostEvictionCallbackRegistration>();

            public CacheItemPriority Priority { get; set; }

            public long? Size { get; set; }

            public void Dispose() => owner.Commit(this);
        }
    }

    [Fact]
    public async Task ConcurrentGetOrCreateAndRemoveByPrefix_LeavesNoStaleEntryAfterAFinalInvalidate()
    {
        // No bounded-time assertion is possible mid-storm (the race is inherently nondeterministic in
        // ordering) - the invariant that must always hold is the one callers actually depend on: once
        // every concurrent writer has finished AND one final RemoveByPrefix has run, the very next read
        // must be a genuine miss, never a leftover from any generation produced during the storm.
        var service = CreateService();
        const string key = "referencedata:list:Sector:true";
        const string prefix = "referencedata:list:Sector:";

        await service.GetOrCreateAsync(key, _ => Task.FromResult("seed"));

        var tasks = new List<Task>();
        for (var i = 0; i < 8; i++)
        {
            var iteration = i;
            tasks.Add(Task.Run(async () =>
            {
                for (var j = 0; j < 50; j++)
                {
                    await service.GetOrCreateAsync(key, _ => Task.FromResult($"v-{iteration}-{j}"));
                    service.RemoveByPrefix(prefix);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Settle any still-in-flight eviction callbacks from the storm before the final, decisive
        // invalidation.
        await Task.Delay(300);
        service.RemoveByPrefix(prefix);

        var callCount = 0;
        await service.GetOrCreateAsync(key, _ =>
        {
            callCount++;
            return Task.FromResult("final");
        });

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
