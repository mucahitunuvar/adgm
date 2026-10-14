using GenclikMerkezi.Modules.Website.Application.LinkTargets;
using GenclikMerkezi.Modules.Website.Features.GetMenuByLocation;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.GetMenuByLocation;

public class GetMenuByLocationQueryHandlerTests
{
    private readonly FakeMenuRepository _menuRepository = new();
    private readonly FakeSiteLanguageRepository _siteLanguageRepository = new();
    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly FakeContentTypeRepository _contentTypeRepository = new();

    private GetMenuByLocationQueryHandler CreateHandler() =>
        new(
            _menuRepository, _siteLanguageRepository, new LinkTargetResolver(_contentItemRepository, _contentTypeRepository),
            TimeProvider.System);

    [Theory]
    [InlineData("999")]
    [InlineData("Header,Utility")] // 0|1 = 1 = Utility - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedLocation_Fails(string location)
    {
        var result = await CreateHandler().Handle(new GetMenuByLocationQuery(location), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Menu.LocationInvalid", result.Error.Code);
    }
}
