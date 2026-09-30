using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class ContentSeoResolverTests
{
    private static readonly SeoMetadata EmptySeo = SeoMetadata.CreateEmpty();

    [Fact]
    public void Resolve_WithNoSeoOverrides_FallsBackToTitleAndSummary()
    {
        var result = ContentSeoResolver.Resolve(EmptySeo, "Başlık", "Özet metni", "/haberler/baslik", null, null, null, "Varsayılan açıklama");

        Assert.Equal("Başlık", result.MetaTitle);
        Assert.Equal("Özet metni", result.MetaDescription);
        Assert.Equal("Başlık", result.OgTitle);
        Assert.Equal("Özet metni", result.OgDescription);
        Assert.False(result.NoIndex);
    }

    [Fact]
    public void Resolve_WithEmptySummary_FallsBackToSiteDefaultDescription()
    {
        var result = ContentSeoResolver.Resolve(EmptySeo, "Başlık", "", "/haberler/baslik", null, null, null, "Varsayılan açıklama");

        Assert.Equal("Varsayılan açıklama", result.MetaDescription);
    }

    [Fact]
    public void Resolve_WithExplicitSeoMetaTitle_OverridesTitle()
    {
        var seo = SeoMetadata.Create("Özel Başlık", null, null, null, null, null, null, false).Value;

        var result = ContentSeoResolver.Resolve(seo, "Başlık", "Özet", "/x", null, null, null, "Varsayılan");

        Assert.Equal("Özel Başlık", result.MetaTitle);
    }

    [Fact]
    public void Resolve_OgImage_PrefersDetailImageOverCoverImage()
    {
        var detailImageId = Guid.NewGuid();
        var coverImageId = Guid.NewGuid();

        var result = ContentSeoResolver.Resolve(EmptySeo, "Başlık", "Özet", "/x", detailImageId, coverImageId, null, "Varsayılan");

        Assert.Equal(detailImageId, result.OgImageMediaId);
    }

    [Fact]
    public void Resolve_OgImage_FallsBackToCoverImageWhenNoDetailImage()
    {
        var coverImageId = Guid.NewGuid();

        var result = ContentSeoResolver.Resolve(EmptySeo, "Başlık", "Özet", "/x", null, coverImageId, null, "Varsayılan");

        Assert.Equal(coverImageId, result.OgImageMediaId);
    }

    [Fact]
    public void Resolve_OgImage_FallsBackToSiteDefaultWhenNoContentImages()
    {
        var siteDefaultId = Guid.NewGuid();

        var result = ContentSeoResolver.Resolve(EmptySeo, "Başlık", "Özet", "/x", null, null, siteDefaultId, "Varsayılan");

        Assert.Equal(siteDefaultId, result.OgImageMediaId);
    }

    [Fact]
    public void Resolve_OgImage_ExplicitSeoOgImage_TakesPriorityOverEverything()
    {
        var seoOgImageId = Guid.NewGuid();
        var seo = SeoMetadata.Create(null, null, null, null, null, seoOgImageId, null, false).Value;

        var result = ContentSeoResolver.Resolve(seo, "Başlık", "Özet", "/x", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Varsayılan");

        Assert.Equal(seoOgImageId, result.OgImageMediaId);
    }

    [Fact]
    public void Resolve_LongSummary_TruncatesAtWordBoundaryTo160Characters()
    {
        var longSummary = string.Join(' ', Enumerable.Repeat("kelime", 40));

        var result = ContentSeoResolver.Resolve(EmptySeo, "Başlık", longSummary, "/x", null, null, null, "Varsayılan");

        Assert.True(result.MetaDescription.Length <= 160);
        Assert.False(result.MetaDescription.EndsWith(' '));
    }

    [Fact]
    public void Resolve_CanonicalUrl_IsTheGivenPath()
    {
        var result = ContentSeoResolver.Resolve(EmptySeo, "Başlık", "Özet", "/tr/haberler/baslik", null, null, null, "Varsayılan");

        Assert.Equal("/tr/haberler/baslik", result.CanonicalUrl);
    }
}
