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
        Assert.Single(result.Value);
        Assert.Equal("TR", result.Value[0].Code);
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

        Assert.Empty(activeOnlyResult.Value);
        Assert.Single(allResult.Value);
    }
}
