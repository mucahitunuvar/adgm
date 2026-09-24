using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class SeoMetadataTests
{
    [Fact]
    public void Create_WithValidValues_Succeeds()
    {
        var result = SeoMetadata.Create(
            "Meta title", "Meta description", "keyword1, keyword2", "Og title", "Og description",
            Guid.NewGuid(), "https://example.org/page", noIndex: false);

        Assert.True(result.IsSuccess);
        Assert.Equal("Meta title", result.Value.MetaTitle);
        Assert.Equal("https://example.org/page", result.Value.CanonicalUrl);
    }

    [Fact]
    public void CreateEmpty_HasEmptyStringsAndNoCanonicalUrl()
    {
        var metadata = SeoMetadata.CreateEmpty();

        Assert.Equal(string.Empty, metadata.MetaTitle);
        Assert.Null(metadata.CanonicalUrl);
        Assert.False(metadata.NoIndex);
    }

    [Fact]
    public void Create_WithMetaTitleTooLong_Fails()
    {
        var result = SeoMetadata.Create(
            new string('a', SeoMetadata.MaxMetaTitleLength + 1), null, null, null, null, null, null, false);

        Assert.True(result.IsFailure);
        Assert.Equal("SeoMetadata.MetaTitleTooLong", result.Error.Code);
    }

    [Fact]
    public void Create_WithMetaDescriptionTooLong_Fails()
    {
        var result = SeoMetadata.Create(
            null, new string('a', SeoMetadata.MaxMetaDescriptionLength + 1), null, null, null, null, null, false);

        Assert.True(result.IsFailure);
        Assert.Equal("SeoMetadata.MetaDescriptionTooLong", result.Error.Code);
    }

    [Fact]
    public void Create_WithOgTitleTooLong_Fails()
    {
        var result = SeoMetadata.Create(
            null, null, null, new string('a', SeoMetadata.MaxOgTitleLength + 1), null, null, null, false);

        Assert.True(result.IsFailure);
        Assert.Equal("SeoMetadata.OgTitleTooLong", result.Error.Code);
    }

    [Fact]
    public void Create_WithOgDescriptionTooLong_Fails()
    {
        var result = SeoMetadata.Create(
            null, null, null, null, new string('a', SeoMetadata.MaxOgDescriptionLength + 1), null, null, false);

        Assert.True(result.IsFailure);
        Assert.Equal("SeoMetadata.OgDescriptionTooLong", result.Error.Code);
    }

    [Fact]
    public void Create_WithMetaKeywordsTooLong_Fails()
    {
        var result = SeoMetadata.Create(
            null, null, new string('a', SeoMetadata.MaxMetaKeywordsLength + 1), null, null, null, null, false);

        Assert.True(result.IsFailure);
        Assert.Equal("SeoMetadata.MetaKeywordsTooLong", result.Error.Code);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.org/page")]
    [InlineData("/relative/path")]
    public void Create_WithInvalidCanonicalUrl_Fails(string canonicalUrl)
    {
        var result = SeoMetadata.Create(null, null, null, null, null, null, canonicalUrl, false);

        Assert.True(result.IsFailure);
        Assert.Equal("SeoMetadata.CanonicalUrlInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_WithNullCanonicalUrl_Succeeds()
    {
        var result = SeoMetadata.Create(null, null, null, null, null, null, null, false);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.CanonicalUrl);
    }
}
