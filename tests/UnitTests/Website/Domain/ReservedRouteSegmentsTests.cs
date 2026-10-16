using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class ReservedRouteSegmentsTests
{
    [Theory]
    [InlineData("api")]
    [InlineData("admin")]
    [InlineData("portal")]
    [InlineData("webuploads")]
    [InlineData("uploads")]
    [InlineData("media")]
    [InlineData("assets")]
    [InlineData("static")]
    [InlineData("sitemap.xml")]
    [InlineData("robots.txt")]
    [InlineData("API")]
    [InlineData("giris")]
    [InlineData("uye-ol")]
    [InlineData("ilanlar")]
    [InlineData("newsletter")]
    [InlineData("NEWSLETTER")]
    public void IsReserved_WithStaticSegment_ReturnsTrue(string segment)
    {
        Assert.True(ReservedRouteSegments.IsReserved(segment, []));
    }

    // Deliberately not reserved - the seeded Event ContentType already owns both as its own
    // RoutePrefix (tr/en); see the comment on ReservedRouteSegments.StaticSegments.
    [Theory]
    [InlineData("events")]
    [InlineData("etkinlikler")]
    public void IsReserved_WithEventContentTypeOwnPrefix_ReturnsFalse(string segment)
    {
        Assert.False(ReservedRouteSegments.IsReserved(segment, ["tr", "en"]));
    }

    [Fact]
    public void IsReserved_WithLanguageCode_ReturnsTrue()
    {
        Assert.True(ReservedRouteSegments.IsReserved("tr", ["tr", "en"]));
        Assert.True(ReservedRouteSegments.IsReserved("EN", ["tr", "en"]));
    }

    [Fact]
    public void IsReserved_WithOrdinarySegment_ReturnsFalse()
    {
        Assert.False(ReservedRouteSegments.IsReserved("haberler", ["tr", "en"]));
    }
}
