using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class SiteThemeTests
{
    [Fact]
    public void Create_WithValidHexColors_Succeeds()
    {
        var result = SiteTheme.Create("#1A2B3C", "#ffffff", "Inter");

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
        var result = SiteTheme.Create(primaryColorHex, null, null);

        Assert.True(result.IsFailure);
        Assert.Equal("SiteTheme.InvalidPrimaryColor", result.Error.Code);
    }

    [Fact]
    public void Create_WithFontFamilyTooLong_Fails()
    {
        var result = SiteTheme.Create(null, null, new string('a', SiteTheme.MaxFontFamilyLength + 1));

        Assert.True(result.IsFailure);
        Assert.Equal("SiteTheme.FontFamilyTooLong", result.Error.Code);
    }

    [Fact]
    public void CreateEmpty_HasEmptyStrings()
    {
        var theme = SiteTheme.CreateEmpty();

        Assert.Equal(string.Empty, theme.PrimaryColorHex);
        Assert.Equal(string.Empty, theme.SecondaryColorHex);
        Assert.Equal(string.Empty, theme.FontFamily);
    }
}
