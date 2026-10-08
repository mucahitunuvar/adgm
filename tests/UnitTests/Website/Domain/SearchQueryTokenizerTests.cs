using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class SearchQueryTokenizerTests
{
    [Fact]
    public void BuildLikePatterns_SplitsOnWhitespace_AndWrapsEachTokenInWildcards()
    {
        var patterns = SearchQueryTokenizer.BuildLikePatterns("kariyer merkezi");

        Assert.Equal(["%KARIYER%", "%MERKEZI%"], patterns);
    }

    [Fact]
    public void BuildLikePatterns_CollapsesRepeatedWhitespace()
    {
        var patterns = SearchQueryTokenizer.BuildLikePatterns("kariyer   merkezi\tgenclik");

        Assert.Equal(["%KARIYER%", "%MERKEZI%", "%GENCLIK%"], patterns);
    }

    [Fact]
    public void BuildLikePatterns_TakesAtMostSixTokens()
    {
        var patterns = SearchQueryTokenizer.BuildLikePatterns("bir iki uc dort bes alti yedi sekiz");

        Assert.Equal(SearchQueryTokenizer.MaxTokenCount, patterns.Count);
        Assert.Equal(["%BIR%", "%IKI%", "%UC%", "%DORT%", "%BES%", "%ALTI%"], patterns);
    }

    [Theory]
    [InlineData("İSTANBUL", "%ISTANBUL%")]
    [InlineData("istanbul", "%ISTANBUL%")]
    [InlineData("ığüşöç", "%IGUSOC%")]
    public void BuildLikePatterns_FoldsTurkishDiacriticsAndCase(string query, string expectedPattern)
    {
        var patterns = SearchQueryTokenizer.BuildLikePatterns(query);

        Assert.Equal([expectedPattern], patterns);
    }

    [Theory]
    [InlineData("50%", "%50\\%%")]
    [InlineData("a_b", "%A\\_B%")]
    [InlineData("a[b", "%A\\[B%")]
    [InlineData(@"a\b", "%A\\\\B%")]
    public void BuildLikePatterns_EscapesLikeWildcardCharacters(string token, string expectedPattern)
    {
        var patterns = SearchQueryTokenizer.BuildLikePatterns(token);

        Assert.Equal([expectedPattern], patterns);
    }
}
