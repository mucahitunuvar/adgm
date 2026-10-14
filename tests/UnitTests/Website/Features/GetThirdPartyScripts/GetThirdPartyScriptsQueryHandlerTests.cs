using GenclikMerkezi.Modules.Website.Features.GetThirdPartyScripts;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.GetThirdPartyScripts;

public class GetThirdPartyScriptsQueryHandlerTests
{
    private readonly FakeThirdPartyScriptRepository _thirdPartyScriptRepository = new();
    private readonly FakeSiteLanguageRepository _siteLanguageRepository = new();

    private GetThirdPartyScriptsQueryHandler CreateHandler() => new(_thirdPartyScriptRepository, _siteLanguageRepository);

    [Theory]
    [InlineData("999")]
    [InlineData("Necessary,Analytics")] // 0|1 = 1 = Analytics - would otherwise silently alias.
    [InlineData("necessary")] // case-sensitive: lowercase must not match 'Necessary'.
    public async Task Handle_WithNumericCombinedOrWrongCaseCategory_Fails(string category)
    {
        var result = await CreateHandler().Handle(new GetThirdPartyScriptsQuery(null, category), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("ThirdPartyScript.CategoryInvalid", result.Error.Code);
    }
}
