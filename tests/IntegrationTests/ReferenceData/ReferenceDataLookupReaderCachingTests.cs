using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.ReferenceData.Application.Abstractions;
using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.Modules.ReferenceData.Infrastructure;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.ReferenceData;

// ADR-017 Decision 3: ReferenceDataLookupReader now caches ListAsync/ListByParentAsync results per
// (type, activeOnly, page, pageSize[, parentId]) via ICacheService, invalidated by
// LookupCacheInvalidator.RemoveByPrefix. This exercises that directly against ReferenceDataDbContext
// (bypassing the admin CRUD command handlers, which already call the invalidator themselves) to
// prove ListAsync is a genuine cache - not just a pass-through - and that invalidation actually
// clears the stale entry, not just the two keys the previous "cache the whole list" design tracked.
public class ReferenceDataLookupReaderCachingTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task ListAsync_ServesStaleDataUntilInvalidated_ThenReflectsTheChange()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ReferenceDataDbContext>();
        var lookupReader = scope.ServiceProvider.GetRequiredService<IReferenceDataLookupReader>();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

        var sector = Sector.Create($"CACHE-{Guid.NewGuid():N}"[..15], "Cache Test Sektörü", 999);
        dbContext.Sectors.Add(sector);
        await dbContext.SaveChangesAsync();

        var paging = new PagedRequest { Page = 1, PageSize = 100 };

        var beforeDeactivate = await lookupReader.ListAsync(ReferenceDataLookupType.Sector, paging, activeOnly: true);
        Assert.Contains(beforeDeactivate.Items, i => i.Id == sector.Id);

        // Deactivate directly against the DB, deliberately bypassing DeactivateLookupItemCommandHandler
        // (which would call LookupCacheInvalidator itself) - isolates whether ListAsync alone is
        // actually caching, independent of the invalidation path.
        sector.Deactivate();
        await dbContext.SaveChangesAsync();

        var stillCached = await lookupReader.ListAsync(ReferenceDataLookupType.Sector, paging, activeOnly: true);
        Assert.Contains(stillCached.Items, i => i.Id == sector.Id);

        LookupCacheInvalidator.Invalidate(cacheService, ReferenceDataLookupType.Sector);

        var afterInvalidate = await lookupReader.ListAsync(ReferenceDataLookupType.Sector, paging, activeOnly: true);
        Assert.DoesNotContain(afterInvalidate.Items, i => i.Id == sector.Id);
    }

    [Fact]
    public async Task ListAsync_DifferentPageSizeCombinations_AreCachedSeparately()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ReferenceDataDbContext>();
        var lookupReader = scope.ServiceProvider.GetRequiredService<IReferenceDataLookupReader>();

        for (var i = 0; i < 3; i++)
        {
            dbContext.Currencies.Add(Currency.Create($"CUR-{Guid.NewGuid():N}"[..10], $"Currency {i}", i));
        }
        await dbContext.SaveChangesAsync();

        var smallPage = await lookupReader.ListAsync(
            ReferenceDataLookupType.Currency, new PagedRequest { Page = 1, PageSize = 1 }, activeOnly: true);
        var largePage = await lookupReader.ListAsync(
            ReferenceDataLookupType.Currency, new PagedRequest { Page = 1, PageSize = 100 }, activeOnly: true);

        // If page/pageSize were not part of the cache key, the second (larger) request could
        // incorrectly reuse the first request's single-item cached page.
        Assert.Single(smallPage.Items);
        Assert.True(largePage.Items.Count > 1);
        Assert.Equal(smallPage.TotalCount, largePage.TotalCount);
    }
}
