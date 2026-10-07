using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class SearchTextBuilderTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("<p>Merhaba <strong>dünya</strong></p>", "Merhaba dünya")]
    [InlineData("<p>Satış &amp; Pazarlama</p>", "Satış & Pazarlama")]
    [InlineData("<ul><li>Bir</li><li>İki</li></ul>", "Bir İki")]
    [InlineData("Satırlar\narası\r\nboşluk   birden  fazla", "Satırlar arası boşluk birden fazla")]
    public void StripHtml_ProducesExpectedPlainText(string? html, string expected)
    {
        Assert.Equal(expected, SearchTextBuilder.StripHtml(html));
    }

    [Fact]
    public void BuildSummary_WithTextShorterThanMaxLength_ReturnsFullText()
    {
        var result = SearchTextBuilder.BuildSummary("<p>Kısa açıklama</p>", 200);

        Assert.Equal("Kısa açıklama", result);
    }

    [Fact]
    public void BuildSummary_WithTextLongerThanMaxLength_TruncatesAtWordBoundary_NeverMidWord()
    {
        var html = "<p>" + string.Join(' ', Enumerable.Repeat("kelime", 50)) + "</p>";

        var result = SearchTextBuilder.BuildSummary(html, 20);

        Assert.True(result.Length <= 20);
        Assert.False(result.EndsWith("kelim", StringComparison.Ordinal));
        Assert.True(result.Split(' ').All(word => word == "kelime"));
    }

    [Fact]
    public void BuildNormalizedText_ConcatenatesTitleSummaryAndBody_AndFoldsTurkishDiacritics()
    {
        var result = SearchTextBuilder.BuildNormalizedText(
            "İstanbul Ofisi", "Şeyma Öztürk", "<p>Çağrı merkezi görevlisi aranıyor</p>", 4000);

        Assert.Equal("ISTANBUL OFISI SEYMA OZTURK CAGRI MERKEZI GOREVLISI ARANIYOR", result);
    }

    [Theory]
    [InlineData("İrem", "irem", true)]
    [InlineData("ırem", "İREM", true)]
    public void BuildNormalizedText_TurkishIVariants_AllFoldToTheSameToken(string titleA, string titleB, bool expectEqual)
    {
        var first = SearchTextBuilder.BuildNormalizedText(titleA, "", null, 4000);
        var second = SearchTextBuilder.BuildNormalizedText(titleB, "", null, 4000);

        Assert.Equal(expectEqual, first == second);
    }

    [Fact]
    public void BuildNormalizedText_LongerThanMaxLength_IsHardTruncated()
    {
        var longTitle = string.Join(' ', Enumerable.Repeat("kelime", 2000));

        var result = SearchTextBuilder.BuildNormalizedText(longTitle, "", null, 100);

        Assert.Equal(100, result.Length);
    }

    [Fact]
    public void BuildNormalizedText_SkipsBlankParts_WithoutLeavingDoubleSpaces()
    {
        var result = SearchTextBuilder.BuildNormalizedText("Başlık", "", null, 4000);

        Assert.Equal("BASLIK", result);
    }

    [Fact]
    public void BuildSummary_IsDeterministic()
    {
        const string html = "<p>Kaynakçı aranıyor</p>";

        var first = SearchTextBuilder.BuildSummary(html, 200);
        var second = SearchTextBuilder.BuildSummary(html, 200);

        Assert.Equal(first, second);
    }
}
