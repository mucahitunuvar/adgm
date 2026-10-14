using GenclikMerkezi.Modules.Website.Application.ThirdPartyScripts;

namespace GenclikMerkezi.UnitTests.Website.Application.ThirdPartyScripts;

public class ThirdPartyScriptProviderResolverTests
{
    [Fact]
    public void Resolve_WithKnownProviderName_Succeeds()
    {
        var result = ThirdPartyScriptProviderResolver.Resolve(
            "GoogleAnalytics4", "G-ABC123", null, null, null, false, false, []);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData("999")]
    [InlineData("GoogleTagManager,MetaPixel")] // 1|2 = 3 = ExternalScript - would otherwise silently alias.
    public void Resolve_WithNumericOrCombinedProvider_Fails(string providerKind)
    {
        var result = ThirdPartyScriptProviderResolver.Resolve(
            providerKind, null, null, null, null, false, false, []);

        Assert.True(result.IsFailure);
        Assert.Equal("ThirdPartyScript.ProviderInvalid", result.Error.Code);
    }
}
