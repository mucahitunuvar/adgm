using GenclikMerkezi.Modules.Website.Features.GetContentItems;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.GetContentItems;

public class GetContentItemsQueryHandlerTests
{
    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly FakeSiteLanguageRepository _siteLanguageRepository = new();

    private GetContentItemsQueryHandler CreateHandler() => new(_contentItemRepository, _siteLanguageRepository);

    [Theory]
    [InlineData("999")]
    [InlineData("Draft,Published")] // 0|1 = 1 = Published - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedStatus_Fails(string status)
    {
        var result = await CreateHandler().Handle(
            new GetContentItemsQuery(null, status, null, null, null, null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItem.InvalidStatusFilter", result.Error.Code);
    }
}
