using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.ReferenceData.Features.GetPublicLookupItems;
using GenclikMerkezi.UnitTests.ReferenceData.TestDoubles;

namespace GenclikMerkezi.UnitTests.ReferenceData.Features.GetPublicLookupItems;

public class GetPublicLookupItemsQueryHandlerTests
{
    private readonly FakeReferenceDataLookupReader _lookupReader = new();

    private GetPublicLookupItemsQueryHandler CreateHandler() => new(_lookupReader);

    [Fact]
    public async Task Handle_ReturnsOnlyIdAndName_NeverCodeOrIsActiveOrSortOrder()
    {
        var active = new LookupItemSummary(Guid.NewGuid(), "IST", "İstanbul", true, 1);
        _lookupReader.SeedList(ReferenceDataLookupType.Province, active);

        var result = await CreateHandler().Handle(new GetPublicLookupItemsQuery(ReferenceDataLookupType.Province), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal(active.Id, item.Id);
        Assert.Equal("İstanbul", item.Name);
    }

    [Fact]
    public async Task Handle_NeverIncludesInactiveItems_EvenThoughQueryHasNoActiveOnlyParameter()
    {
        var inactive = new LookupItemSummary(Guid.NewGuid(), "OLD", "Eski Departman", false, 0);
        _lookupReader.SeedList(ReferenceDataLookupType.Department, inactive);

        var result = await CreateHandler().Handle(new GetPublicLookupItemsQuery(ReferenceDataLookupType.Department), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
    }

    [Fact]
    public async Task Handle_WithMoreItemsThanPageSize_ReturnsCorrectSliceAndTotalPages()
    {
        var items = Enumerable.Range(0, 25)
            .Select(i => new LookupItemSummary(Guid.NewGuid(), $"P{i}", $"Pozisyon {i}", true, i))
            .ToArray();
        _lookupReader.SeedList(ReferenceDataLookupType.Position, items);

        var result = await CreateHandler().Handle(
            new GetPublicLookupItemsQuery(ReferenceDataLookupType.Position) { Page = 2, PageSize = 20 },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(25, result.Value.TotalCount);
        Assert.Equal(2, result.Value.TotalPages);
        Assert.Equal(5, result.Value.Items.Count);
    }
}
