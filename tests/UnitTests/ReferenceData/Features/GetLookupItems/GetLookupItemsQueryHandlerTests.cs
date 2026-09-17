using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.ReferenceData.Features.GetLookupItems;
using GenclikMerkezi.UnitTests.ReferenceData.TestDoubles;

namespace GenclikMerkezi.UnitTests.ReferenceData.Features.GetLookupItems;

public class GetLookupItemsQueryHandlerTests
{
    private readonly FakeReferenceDataLookupReader _lookupReader = new();

    private GetLookupItemsQueryHandler CreateHandler() => new(_lookupReader);

    [Fact]
    public async Task Handle_DelegatesToLookupReader_AndReturnsItsResult()
    {
        var active = new LookupItemSummary(Guid.NewGuid(), "TR", "Türkiye", true, 0);
        _lookupReader.SeedList(ReferenceDataLookupType.Country, active);

        var result = await CreateHandler().Handle(
            new GetLookupItemsQuery(ReferenceDataLookupType.Country, ActiveOnly: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal("TR", result.Value.Items[0].Code);
        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task Handle_WithActiveOnlyFalse_IncludesInactiveItems()
    {
        var inactive = new LookupItemSummary(Guid.NewGuid(), "OLD", "Eski Sektör", false, 0);
        _lookupReader.SeedList(ReferenceDataLookupType.Sector, inactive);

        var activeOnlyResult = await CreateHandler().Handle(
            new GetLookupItemsQuery(ReferenceDataLookupType.Sector, ActiveOnly: true), CancellationToken.None);
        var allResult = await CreateHandler().Handle(
            new GetLookupItemsQuery(ReferenceDataLookupType.Sector, ActiveOnly: false), CancellationToken.None);

        Assert.Empty(activeOnlyResult.Value.Items);
        Assert.Single(allResult.Value.Items);
    }

    [Fact]
    public async Task Handle_WithMoreItemsThanPageSize_ReturnsCorrectSliceAndTotalPages()
    {
        var items = Enumerable.Range(0, 25)
            .Select(i => new LookupItemSummary(Guid.NewGuid(), $"C{i}", $"Currency {i}", true, i))
            .ToArray();
        _lookupReader.SeedList(ReferenceDataLookupType.Currency, items);

        var result = await CreateHandler().Handle(
            new GetLookupItemsQuery(ReferenceDataLookupType.Currency, ActiveOnly: true) { Page = 2, PageSize = 20 },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(25, result.Value.TotalCount);
        Assert.Equal(2, result.Value.TotalPages);
        Assert.Equal(5, result.Value.Items.Count);
        Assert.False(result.Value.HasNextPage);
        Assert.True(result.Value.HasPreviousPage);
    }

    [Fact]
    public async Task Handle_WithFewerItemsThanPageSize_ReturnsSinglePage()
    {
        var items = Enumerable.Range(0, 3)
            .Select(i => new LookupItemSummary(Guid.NewGuid(), $"L{i}", $"Language {i}", true, i))
            .ToArray();
        _lookupReader.SeedList(ReferenceDataLookupType.Language, items);

        var result = await CreateHandler().Handle(
            new GetLookupItemsQuery(ReferenceDataLookupType.Language, ActiveOnly: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalCount);
        Assert.Equal(1, result.Value.TotalPages);
        Assert.False(result.Value.HasNextPage);
        Assert.False(result.Value.HasPreviousPage);
    }
}
