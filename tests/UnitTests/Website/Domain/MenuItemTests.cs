using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class MenuItemTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;

    private static MenuItemTranslation ValidTranslation() => MenuItemTranslation.Create(Tr, "Anasayfa").Value;

    [Fact]
    public void Create_WithValidInput_Succeeds()
    {
        var result = MenuItem.Create(
            Guid.NewGuid(), null, 1, true, LinkTarget.CreateEmpty(), false, "home", [ValidTranslation()]);

        Assert.True(result.IsSuccess);
        Assert.Equal("home", result.Value.IconKey);
    }

    [Theory]
    [InlineData("Home")]
    [InlineData("home_page")]
    [InlineData("home page")]
    public void Create_WithInvalidIconKey_Fails(string iconKey)
    {
        var result = MenuItem.Create(
            Guid.NewGuid(), null, 1, true, LinkTarget.CreateEmpty(), false, iconKey, [ValidTranslation()]);

        Assert.True(result.IsFailure);
        Assert.Equal("MenuItem.IconKeyInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_WithIconKeyLongerThanMax_Fails()
    {
        var iconKey = new string('a', MenuItem.MaxIconKeyLength + 1);

        var result = MenuItem.Create(
            Guid.NewGuid(), null, 1, true, LinkTarget.CreateEmpty(), false, iconKey, [ValidTranslation()]);

        Assert.True(result.IsFailure);
        Assert.Equal("MenuItem.IconKeyInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_WithNoTranslations_Fails()
    {
        var result = MenuItem.Create(Guid.NewGuid(), null, 1, true, LinkTarget.CreateEmpty(), false, null, []);

        Assert.True(result.IsFailure);
        Assert.Equal("MenuItem.TranslationsRequired", result.Error.Code);
    }

    [Fact]
    public void Create_WithNullIconKey_Succeeds()
    {
        var result = MenuItem.Create(Guid.NewGuid(), null, 1, true, LinkTarget.CreateEmpty(), false, null, [ValidTranslation()]);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.IconKey);
    }
}
