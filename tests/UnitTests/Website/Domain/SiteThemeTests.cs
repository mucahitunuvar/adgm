using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class SiteThemeTests
{
    [Fact]
    public void Create_WithValidHexColors_Succeeds()
    {
        var result = SiteTheme.Create(null, null, null, "#1A2B3C", "#ffffff", "Inter");

        Assert.True(result.IsSuccess);
        Assert.Equal("#1A2B3C", result.Value.PrimaryColorHex);
        Assert.Equal("#ffffff", result.Value.SecondaryColorHex);
    }

    [Theory]
    [InlineData("1A2B3C")]
    [InlineData("#1A2B3")]
    [InlineData("not-a-color")]
    public void Create_WithInvalidPrimaryColor_Fails(string primaryColorHex)
    {
        var result = SiteTheme.Create(null, null, null, primaryColorHex, null, null);

        Assert.True(result.IsFailure);
        Assert.Equal("SiteTheme.InvalidPrimaryColor", result.Error.Code);
    }

    [Fact]
    public void Create_WithFontFamilyTooLong_Fails()
    {
        var result = SiteTheme.Create(null, null, null, null, null, new string('a', SiteTheme.MaxFontFamilyLength + 1));

        Assert.True(result.IsFailure);
        Assert.Equal("SiteTheme.FontFamilyTooLong", result.Error.Code);
    }

    [Fact]
    public void CreateEmpty_HasNoLogosAndEmptyStrings()
    {
        var theme = SiteTheme.CreateEmpty();

        Assert.Null(theme.LogoLightMediaAssetId);
        Assert.Equal(string.Empty, theme.PrimaryColorHex);
    }
}
