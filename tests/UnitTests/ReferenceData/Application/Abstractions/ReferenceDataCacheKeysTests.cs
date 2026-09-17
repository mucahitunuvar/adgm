using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.ReferenceData.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.ReferenceData.Application.Abstractions;

public class ReferenceDataCacheKeysTests
{
    [Fact]
    public void List_DifferentPages_ProduceDifferentKeys()
    {
        var page1 = ReferenceDataCacheKeys.List(ReferenceDataLookupType.Sector, activeOnly: true, new PagedRequest { Page = 1 });
        var page2 = ReferenceDataCacheKeys.List(ReferenceDataLookupType.Sector, activeOnly: true, new PagedRequest { Page = 2 });

        Assert.NotEqual(page1, page2);
    }

    [Fact]
    public void List_DifferentPageSizes_ProduceDifferentKeys()
    {
        var small = ReferenceDataCacheKeys.List(ReferenceDataLookupType.Sector, activeOnly: true, new PagedRequest { PageSize = 10 });
        var large = ReferenceDataCacheKeys.List(ReferenceDataLookupType.Sector, activeOnly: true, new PagedRequest { PageSize = 20 });

        Assert.NotEqual(small, large);
    }

    [Fact]
    public void List_DifferentActiveOnly_ProduceDifferentKeys()
    {
        var active = ReferenceDataCacheKeys.List(ReferenceDataLookupType.Sector, activeOnly: true, new PagedRequest());
        var all = ReferenceDataCacheKeys.List(ReferenceDataLookupType.Sector, activeOnly: false, new PagedRequest());

        Assert.NotEqual(active, all);
    }

    [Fact]
    public void ListByParent_DifferentParentIds_ProduceDifferentKeys()
    {
        var provinceA = ReferenceDataCacheKeys.ListByParent(
            ReferenceDataLookupType.District, Guid.NewGuid(), activeOnly: true, new PagedRequest());
        var provinceB = ReferenceDataCacheKeys.ListByParent(
            ReferenceDataLookupType.District, Guid.NewGuid(), activeOnly: true, new PagedRequest());

        Assert.NotEqual(provinceA, provinceB);
    }

    [Fact]
    public void List_AndListByParent_ForTheSameType_ShareTheInvalidationPrefix()
    {
        var listKey = ReferenceDataCacheKeys.List(ReferenceDataLookupType.District, activeOnly: true, new PagedRequest());
        var listByParentKey = ReferenceDataCacheKeys.ListByParent(
            ReferenceDataLookupType.District, Guid.NewGuid(), activeOnly: true, new PagedRequest());
        var prefix = ReferenceDataCacheKeys.InvalidationPrefix(ReferenceDataLookupType.District);

        Assert.StartsWith(prefix, listKey);
        Assert.StartsWith(prefix, listByParentKey);
    }

    [Fact]
    public void InvalidationPrefix_DifferentTypes_DoNotOverlap()
    {
        var districtPrefix = ReferenceDataCacheKeys.InvalidationPrefix(ReferenceDataLookupType.District);
        var provincePrefix = ReferenceDataCacheKeys.InvalidationPrefix(ReferenceDataLookupType.Province);
        var provinceKey = ReferenceDataCacheKeys.List(ReferenceDataLookupType.Province, activeOnly: true, new PagedRequest());

        Assert.NotEqual(districtPrefix, provincePrefix);
        Assert.False(
            provinceKey.StartsWith(districtPrefix, StringComparison.Ordinal),
            "a Province cache key must not be accidentally cleared by RemoveByPrefix(District's prefix)");
    }
}
