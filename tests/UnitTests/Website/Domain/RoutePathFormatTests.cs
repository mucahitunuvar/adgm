using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class RoutePathFormatTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("/", "")]
    [InlineData("/Haberler/Yeni-Proje", "haberler/yeni-proje")]
    [InlineData("/haberler/yeni-proje/", "haberler/yeni-proje")]
    [InlineData("//haberler///yeni-proje//", "haberler/yeni-proje")]
    [InlineData("  /haberler  ", "haberler")]
    public void Normalize_ProducesLowercaseSingleSlashNoLeadingOrTrailingSlash(string? rawPath, string expected)
    {
        Assert.Equal(expected, RoutePathFormat.Normalize(rawPath));
    }

    [Theory]
    [InlineData("", "", "")]
    [InlineData("haberler", "haberler", "")]
    [InlineData("haberler/yeni-proje", "haberler", "yeni-proje")]
    [InlineData("en/news/x", "en", "news/x")]
    public void SplitFirstSegment_SplitsOnFirstSlash(string normalizedPath, string expectedFirst, string expectedRemainder)
    {
        var (first, remainder) = RoutePathFormat.SplitFirstSegment(normalizedPath);

        Assert.Equal(expectedFirst, first);
        Assert.Equal(expectedRemainder, remainder);
    }

    [Fact]
    public void BuildPublicPath_ForDefaultLanguageAndEmptyRemainder_ReturnsRoot()
    {
        Assert.Equal("/", RoutePathFormat.BuildPublicPath("tr", "tr", string.Empty));
    }

    [Fact]
    public void BuildPublicPath_ForDefaultLanguageWithRemainder_OmitsLanguageSegment()
    {
        Assert.Equal("/haberler/yeni-proje", RoutePathFormat.BuildPublicPath("tr", "tr", "haberler/yeni-proje"));
    }

    [Fact]
    public void BuildPublicPath_ForNonDefaultLanguage_PrependsLanguageSegment()
    {
        Assert.Equal("/en/news/x", RoutePathFormat.BuildPublicPath("en", "tr", "news/x"));
    }

    [Fact]
    public void BuildPublicPath_ForNonDefaultLanguageWithEmptyRemainder_ReturnsLanguageRootOnly()
    {
        Assert.Equal("/en", RoutePathFormat.BuildPublicPath("en", "tr", string.Empty));
    }
}
