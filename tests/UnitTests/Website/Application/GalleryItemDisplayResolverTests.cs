using GenclikMerkezi.Modules.Website.Application.Media;

namespace GenclikMerkezi.UnitTests.Website.Application;

public class GalleryItemDisplayResolverTests
{
    [Fact]
    public void ResolveAltText_WithOverride_ReturnsOverride()
    {
        var result = GalleryItemDisplayResolver.ResolveAltText("Override alt", "Media alt");

        Assert.Equal("Override alt", result);
    }

    [Fact]
    public void ResolveAltText_WithoutOverride_ReturnsMediaAssetAltText()
    {
        var result = GalleryItemDisplayResolver.ResolveAltText(null, "Media alt");

        Assert.Equal("Media alt", result);
    }

    [Fact]
    public void ResolveAltText_WithNeitherOverrideNorMediaAltText_ReturnsEmptyString()
    {
        var result = GalleryItemDisplayResolver.ResolveAltText(null, null);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ResolveAltText_WithBlankOverride_FallsBackToMediaAssetAltText()
    {
        var result = GalleryItemDisplayResolver.ResolveAltText("   ", "Media alt");

        Assert.Equal("Media alt", result);
    }

    [Fact]
    public void ResolveCaption_WithOverride_ReturnsOverride()
    {
        var result = GalleryItemDisplayResolver.ResolveCaption("Override caption", "Media caption");

        Assert.Equal("Override caption", result);
    }

    [Fact]
    public void ResolveCaption_WithoutOverride_ReturnsMediaAssetCaption()
    {
        var result = GalleryItemDisplayResolver.ResolveCaption(null, "Media caption");

        Assert.Equal("Media caption", result);
    }
}
