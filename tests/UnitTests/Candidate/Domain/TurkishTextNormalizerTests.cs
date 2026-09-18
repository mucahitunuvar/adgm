using GenclikMerkezi.Modules.Candidate.Domain;

namespace GenclikMerkezi.UnitTests.Candidate.Domain;

public class TurkishTextNormalizerTests
{
    [Theory]
    [InlineData("İrem", "IREM")]
    [InlineData("irem", "IREM")]
    [InlineData("Irem", "IREM")]
    [InlineData("ırem", "IREM")]
    public void Normalize_FoldsAllFourVariantsOfTurkishI_ToTheSameResult(string input, string expected)
    {
        Assert.Equal(expected, TurkishTextNormalizer.Normalize(input));
    }

    [Fact]
    public void Normalize_FoldsOtherTurkishDiacritics()
    {
        var result = TurkishTextNormalizer.Normalize("Şeyma Öztürk Çağlar Güneş");

        Assert.Equal("SEYMA OZTURK CAGLAR GUNES", result);
    }

    [Fact]
    public void Normalize_DoesNotProduceCombiningCharacters_ForCapitalDottedI()
    {
        // The classic pitfall: "İ".ToLowerInvariant() yields "i" + a combining dot above (2 chars),
        // not a plain "i" - Normalize must never let that leak into the result.
        var result = TurkishTextNormalizer.Normalize("İ");

        Assert.Equal("I", result);
        Assert.Equal(1, result.Length);
    }

    [Fact]
    public void Normalize_TrimsWhitespace()
    {
        var result = TurkishTextNormalizer.Normalize("  Ahmet Yılmaz  ");

        Assert.Equal("AHMET YILMAZ", result);
    }

    [Fact]
    public void Normalize_TwoDifferentlyCasedSpellingsOfTheSameName_ProduceTheSameResult()
    {
        var first = TurkishTextNormalizer.Normalize("İrem Şahin");
        var second = TurkishTextNormalizer.Normalize("irem şahin");

        Assert.Equal(first, second);
    }
}
