using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class ContentItemDuplicateSuffixesTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LanguageCode En = LanguageCode.Create("en").Value;

    [Fact]
    public void SlugSuffixWord_WithTurkishDefaultLanguage_ReturnsKopya()
    {
        Assert.Equal("kopya", ContentItemDuplicateSuffixes.SlugSuffixWord(Tr));
    }

    [Fact]
    public void SlugSuffixWord_WithNonTurkishDefaultLanguage_ReturnsCopy()
    {
        Assert.Equal("copy", ContentItemDuplicateSuffixes.SlugSuffixWord(En));
    }

    [Fact]
    public void TitleSuffix_ForTurkish_ReturnsKopyaSuffix()
    {
        Assert.Equal(" (Kopya)", ContentItemDuplicateSuffixes.TitleSuffix(Tr));
    }

    [Fact]
    public void TitleSuffix_ForNonTurkish_ReturnsCopySuffix()
    {
        Assert.Equal(" (Copy)", ContentItemDuplicateSuffixes.TitleSuffix(En));
    }
}
