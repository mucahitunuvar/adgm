using GenclikMerkezi.Modules.Website.Features.GetPopups;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.GetPopups;

public class GetPopupsQueryHandlerTests
{
    private readonly FakePopupRepository _popupRepository = new();
    private readonly FakeSiteLanguageRepository _siteLanguageRepository = new();

    private GetPopupsQueryHandler CreateHandler() => new(_popupRepository, _siteLanguageRepository);

    [Theory]
    [InlineData("999")]
    [InlineData("Modal,Banner")] // 0|1 = 1 = Banner - would otherwise silently alias.
    public async Task Handle_WithNumericOrCombinedDisplayMode_Fails(string displayMode)
    {
        var result = await CreateHandler().Handle(new GetPopupsQuery(displayMode, null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Popup.DisplayModeInvalid", result.Error.Code);
    }
}
