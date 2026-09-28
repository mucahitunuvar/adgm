using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class ContentItemTranslationTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly SeoMetadata EmptySeo = SeoMetadata.CreateEmpty();

    [Fact]
    public void Create_WithRoutePrefix_ComputesFullPathAsPrefixSlashSlug()
    {
        var result = ContentItemTranslation.Create(Tr, "Yeni Haberimiz", null, "haberler", [], null, null, EmptySeo);

        Assert.True(result.IsSuccess);
        Assert.Equal("yeni-haberimiz", result.Value.Slug);
        Assert.Equal("haberler/yeni-haberimiz", result.Value.FullPath);
    }

    [Fact]
    public void Create_WithEmptyRoutePrefix_ComputesFullPathAsSlugOnly()
    {
        var result = ContentItemTranslation.Create(Tr, "Hakkımızda", null, string.Empty, [], null, null, EmptySeo);

        Assert.True(result.IsSuccess);
        Assert.Equal("hakkimizda", result.Value.FullPath);
    }

    [Fact]
    public void Create_WithAncestorSlugs_ComputesFullPathInRootToLeafOrder()
    {
        var result = ContentItemTranslation.Create(Tr, "Alt Haber", null, "haberler", ["kategori-a", "kategori-b"], null, null, EmptySeo);

        Assert.True(result.IsSuccess);
        Assert.Equal("haberler/kategori-a/kategori-b/alt-haber", result.Value.FullPath);
    }

    [Fact]
    public void Create_WithExplicitSlug_UsesItInsteadOfDerivingFromTitle()
    {
        var result = ContentItemTranslation.Create(Tr, "Yeni Haberimiz", "ozel-slug", "haberler", [], null, null, EmptySeo);

        Assert.True(result.IsSuccess);
        Assert.Equal("ozel-slug", result.Value.Slug);
        Assert.Equal("haberler/ozel-slug", result.Value.FullPath);
    }

    [Fact]
    public void Create_WithoutTitle_Fails()
    {
        var result = ContentItemTranslation.Create(Tr, null, null, "haberler", [], null, null, EmptySeo);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItemTranslation.TitleInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_WithSummaryTooLong_Fails()
    {
        var result = ContentItemTranslation.Create(
            Tr, "Başlık", null, "haberler", [], new string('a', ContentItemTranslation.MaxSummaryLength + 1), null, EmptySeo);

        Assert.True(result.IsFailure);
        Assert.Equal("ContentItemTranslation.SummaryTooLong", result.Error.Code);
    }

    [Fact]
    public void Update_RecomputesFullPathWhenSlugChanges()
    {
        // Update is internal to ContentItemTranslation - exercised the same way production code
        // reaches it, through the owning ContentItem's SetTranslation (see ContentItemTests for the
        // aggregate-level behavior this drives).
        var item = ContentItem.Create(
            Guid.NewGuid(), null, false, 1, false, null, null, Tr, "Başlık", null, "haberler", [], null, null, EmptySeo,
            Guid.NewGuid(), DateTime.UtcNow).Value;

        var result = item.SetTranslation(Tr, "Başlık", "yeni-slug", "haberler", [], null, null, EmptySeo, Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal("yeni-slug", item.Translations[0].Slug);
        Assert.Equal("haberler/yeni-slug", item.Translations[0].FullPath);
    }
}
