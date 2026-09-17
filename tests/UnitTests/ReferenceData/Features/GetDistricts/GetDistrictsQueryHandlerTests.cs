using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.ReferenceData.Features.GetDistricts;
using GenclikMerkezi.UnitTests.ReferenceData.TestDoubles;

namespace GenclikMerkezi.UnitTests.ReferenceData.Features.GetDistricts;

public class GetDistrictsQueryHandlerTests
{
    private readonly FakeReferenceDataLookupReader _lookupReader = new();

    private GetDistrictsQueryHandler CreateHandler() => new(_lookupReader);

    [Fact]
    public async Task Handle_WithProvinceId_UsesParentScopedLookup_AndExcludesOtherProvinces()
    {
        var istanbulId = Guid.NewGuid();
        var ankaraId = Guid.NewGuid();
        var kadikoy = new LookupItemSummary(Guid.NewGuid(), "34-KADIKOY", "Kadıköy", true, 0);
        var cankaya = new LookupItemSummary(Guid.NewGuid(), "06-CANKAYA", "Çankaya", true, 0);

        _lookupReader.SeedListByParent(ReferenceDataLookupType.District, istanbulId, kadikoy);
        _lookupReader.SeedListByParent(ReferenceDataLookupType.District, ankaraId, cankaya);

        var result = await CreateHandler().Handle(
            new GetDistrictsQuery(istanbulId, ActiveOnly: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal("Kadıköy", result.Value.Items[0].DisplayName);
    }

    [Fact]
    public async Task Handle_WithoutProvinceId_UsesUnscopedLookup_AndReturnsEveryProvincesDistricts()
    {
        var kadikoy = new LookupItemSummary(Guid.NewGuid(), "34-KADIKOY", "Kadıköy", true, 0);
        _lookupReader.SeedList(ReferenceDataLookupType.District, kadikoy);

        var result = await CreateHandler().Handle(
            new GetDistrictsQuery(ProvinceId: null, ActiveOnly: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal("Kadıköy", result.Value.Items[0].DisplayName);
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectSliceAndTotals()
    {
        var provinceId = Guid.NewGuid();
        var districts = Enumerable.Range(0, 5)
            .Select(i => new LookupItemSummary(Guid.NewGuid(), $"D{i}", $"İlçe {i}", true, i))
            .ToArray();
        _lookupReader.SeedListByParent(ReferenceDataLookupType.District, provinceId, districts);

        var result = await CreateHandler().Handle(
            new GetDistrictsQuery(provinceId, ActiveOnly: true) { Page = 2, PageSize = 2 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(3, result.Value.TotalPages);
    }
}
