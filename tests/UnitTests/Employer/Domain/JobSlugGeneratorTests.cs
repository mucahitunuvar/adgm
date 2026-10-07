using GenclikMerkezi.Modules.Employer.Domain;

namespace GenclikMerkezi.UnitTests.Employer.Domain;

public class JobSlugGeneratorTests
{
    private static readonly Guid JobId = Guid.Parse("a1b2c3d4-e5f6-4789-9abc-def012345678");

    [Theory]
    [InlineData("Kaynakçı", "kaynakci")]
    [InlineData("Yazılım Mühendisi Çağrılıyor", "yazilim-muhendisi-cagriliyor")]
    [InlineData("İşçi Alınacaktır", "isci-alinacaktir")]
    [InlineData("Satış & Pazarlama (Uzman!!!)", "satis-pazarlama-uzman")]
    [InlineData("   Depo Görevlisi   ", "depo-gorevlisi")]
    [InlineData("", "ilan")]
    [InlineData("!!!???...", "ilan")]
    public void Generate_ProducesExpectedTitleSlug(string title, string expectedTitleSlug)
    {
        var slug = JobSlugGenerator.Generate(title, JobId);

        Assert.Equal($"{expectedTitleSlug}-a1b2c3d4", slug);
    }

    [Fact]
    public void Generate_WithLongTitle_TruncatesTitlePartTo60Characters_WithoutTrailingHyphen()
    {
        var longTitle = string.Concat(Enumerable.Repeat("Kaynakçı ", 20));

        var slug = JobSlugGenerator.Generate(longTitle, JobId);
        var titlePart = slug[..^9];

        Assert.True(titlePart.Length <= 60);
        Assert.False(titlePart.EndsWith('-'));
        Assert.Equal($"{titlePart}-a1b2c3d4", slug);
    }

    [Fact]
    public void Generate_AppendsFirst8HexCharactersOfJobId()
    {
        var slug = JobSlugGenerator.Generate("Kaynakçı", JobId);

        Assert.EndsWith("-a1b2c3d4", slug);
    }

    [Fact]
    public void Generate_CollapsesConsecutiveSeparatorsIntoSingleHyphen()
    {
        var slug = JobSlugGenerator.Generate("Ön   Muhasebe---Elemanı", JobId);

        Assert.Equal("on-muhasebe-elemani-a1b2c3d4", slug);
    }

    [Fact]
    public void Generate_IsDeterministic_ForSameTitleAndId()
    {
        var first = JobSlugGenerator.Generate("Kaynakçı", JobId);
        var second = JobSlugGenerator.Generate("Kaynakçı", JobId);

        Assert.Equal(first, second);
    }
}
