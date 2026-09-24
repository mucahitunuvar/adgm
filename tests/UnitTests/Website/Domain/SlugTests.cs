using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class SlugTests
{
    [Theory]
    [InlineData("Çağrı Merkezi Şubesi İş İlanı", "cagri-merkezi-subesi-is-ilani")]
    [InlineData("Öğrenci Kulübü Üyelik Başvurusu", "ogrenci-kulubu-uyelik-basvurusu")]
    [InlineData("Gönüllülük Fırsatları", "gonulluluk-firsatlari")]
    [InlineData("İstanbul'da Staj İlanı", "istanbul-da-staj-ilani")]
    [InlineData("Kariyer Danışmanlığı", "kariyer-danismanligi")]
    public void Create_FromTurkishTitle_ProducesExpectedSlug(string title, string expected)
    {
        var result = Slug.Create(title);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Theory]
    [InlineData("café résumé", "cafe-resume")]
    [InlineData("naïve café", "naive-cafe")]
    public void Create_FromNonTurkishAccentedText_StripsAccents(string title, string expected)
    {
        var result = Slug.Create(title);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Theory]
    [InlineData("  already-a-slug  ", "already-a-slug")]
    [InlineData("Multiple   Spaces___Here", "multiple-spaces-here")]
    [InlineData("--leading-and-trailing--", "leading-and-trailing")]
    [InlineData("a/b\\c?d#e", "a-b-c-d-e")]
    public void Create_NormalizesUserProvidedSlug_SameAsGeneratingFromATitle(string input, string expected)
    {
        var result = Slug.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!!!???")]
    public void Create_WithNothingLeftAfterNormalization_Fails(string? input)
    {
        var result = Slug.Create(input);

        Assert.True(result.IsFailure);
        Assert.Equal("Slug.Empty", result.Error.Code);
    }

    [Fact]
    public void Create_LongerThanMaxLength_TruncatesAtTheNearestWordBoundary()
    {
        var words = Enumerable.Range(1, 60).Select(i => $"kelime{i}");
        var title = string.Join(" ", words);

        var result = Slug.Create(title);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Value.Length <= Slug.MaxLength);
        Assert.False(result.Value.Value.EndsWith('-'));
    }

    [Fact]
    public void Create_WithOneGiantWordLongerThanMaxLength_HardTruncates()
    {
        var title = new string('a', Slug.MaxLength + 50);

        var result = Slug.Create(title);

        Assert.True(result.IsSuccess);
        Assert.Equal(Slug.MaxLength, result.Value.Value.Length);
    }
}
