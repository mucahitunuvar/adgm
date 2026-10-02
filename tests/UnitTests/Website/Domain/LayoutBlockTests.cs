using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class LayoutBlockTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LanguageCode En = LanguageCode.Create("en").Value;

    [Fact]
    public void Create_WithEmptyBlockTypeKey_Fails()
    {
        var result = LayoutBlock.Create(string.Empty, 1, true, "{}", []);

        Assert.True(result.IsFailure);
        Assert.Equal("LayoutBlock.BlockTypeKeyRequired", result.Error.Code);
    }

    [Fact]
    public void Create_WithDuplicateTranslationLanguage_Fails()
    {
        var translations = new[]
        {
            LayoutBlockTranslation.Create(Tr, "{}"),
            LayoutBlockTranslation.Create(Tr, "{}"),
        };

        var result = LayoutBlock.Create("rich-text", 1, true, "{}", translations);

        Assert.True(result.IsFailure);
        Assert.Equal("LayoutBlock.DuplicateTranslationLanguage", result.Error.Code);
    }

    [Fact]
    public void Create_WithDistinctLanguages_Succeeds()
    {
        var translations = new[]
        {
            LayoutBlockTranslation.Create(Tr, "{\"body\":\"tr\"}"),
            LayoutBlockTranslation.Create(En, "{\"body\":\"en\"}"),
        };

        var result = LayoutBlock.Create("rich-text", 1, true, "{}", translations);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Translations.Count);
    }
}
