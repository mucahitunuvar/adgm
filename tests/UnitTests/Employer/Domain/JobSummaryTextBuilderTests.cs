using GenclikMerkezi.Modules.Employer.Domain;

namespace GenclikMerkezi.UnitTests.Employer.Domain;

public class JobSummaryTextBuilderTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("<p>Merhaba <strong>dünya</strong></p>", "Merhaba dünya")]
    [InlineData("<p>Satış &amp; Pazarlama</p>", "Satış & Pazarlama")]
    [InlineData("<ul><li>Bir</li><li>İki</li></ul>", "Bir İki")]
    [InlineData("Satırlar\narası\r\nboşluk   birden  fazla", "Satırlar arası boşluk birden fazla")]
    public void Build_ProducesExpectedPlainText(string? html, string expected)
    {
        var result = JobSummaryTextBuilder.Build(html, 200);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Build_WithTextShorterThanMaxLength_ReturnsFullText()
    {
        var result = JobSummaryTextBuilder.Build("<p>Kısa açıklama</p>", 200);

        Assert.Equal("Kısa açıklama", result);
    }

    [Fact]
    public void Build_WithTextLongerThanMaxLength_TruncatesAtWordBoundary_NeverMidWord()
    {
        var html = "<p>" + string.Join(' ', Enumerable.Repeat("kelime", 50)) + "</p>";

        var result = JobSummaryTextBuilder.Build(html, 20);

        Assert.True(result.Length <= 20);
        Assert.False(result.EndsWith("kelim", StringComparison.Ordinal));
        Assert.True(result.Split(' ').All(word => word == "kelime"));
    }

    [Fact]
    public void Build_RemovesScriptTagContentAsPlainText_NoMarkupSurvives()
    {
        var result = JobSummaryTextBuilder.Build("<p>Önce</p><script>alert(1)</script><p>Sonra</p>", 200);

        Assert.DoesNotContain("<", result, StringComparison.Ordinal);
        Assert.Contains("Önce", result);
        Assert.Contains("Sonra", result);
    }

    [Fact]
    public void Build_IsDeterministic()
    {
        const string html = "<p>Kaynakçı aranıyor</p>";

        var first = JobSummaryTextBuilder.Build(html, 200);
        var second = JobSummaryTextBuilder.Build(html, 200);

        Assert.Equal(first, second);
    }
}
