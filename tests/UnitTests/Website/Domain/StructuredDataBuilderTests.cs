using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

// ADR-024 §15 (Faz 5 Görev 6): pure field-mapping coverage for every schema.org shape
// StructuredDataBuilder produces - required fields present, optional fields omitted when empty
// ("boş alanlar yazılmaz"), OnlineLink never surfacing in Event output, and FAQ answers reduced to
// plain text.
public class StructuredDataBuilderTests
{
    [Fact]
    public void BuildBreadcrumbList_NumbersPositionsStartingAtOne()
    {
        var jsonLd = StructuredDataBuilder.BuildBreadcrumbList(
            [("Ana Sayfa", "https://example.org/"), ("Haberler", "https://example.org/haberler"), ("Başlık", "https://example.org/haberler/x")]);

        Assert.Equal("https://schema.org", jsonLd["@context"]);
        Assert.Equal("BreadcrumbList", jsonLd["@type"]);
        var items = Assert.IsAssignableFrom<IReadOnlyList<Dictionary<string, object?>>>(jsonLd["itemListElement"]);
        Assert.Equal(3, items.Count);
        Assert.Equal(1, items[0]["position"]);
        Assert.Equal("Ana Sayfa", items[0]["name"]);
        Assert.Equal("https://example.org/", items[0]["item"]);
        Assert.Equal(3, items[2]["position"]);
    }

    [Fact]
    public void BuildArticle_WithEveryFieldPresent_IncludesImageAndDateModifiedAndPublisherLogo()
    {
        var jsonLd = StructuredDataBuilder.BuildArticle(
            ContentSchemaKind.NewsArticle, "Başlık", "Açıklama", "https://example.org/img.jpg", new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 1, 2, 11, 0, 0, DateTimeKind.Utc), "https://example.org/haberler/x", "Gençlik Merkezi",
            "https://example.org/logo.png");

        Assert.Equal("NewsArticle", jsonLd["@type"]);
        Assert.Equal("Başlık", jsonLd["headline"]);
        Assert.Equal("Açıklama", jsonLd["description"]);
        Assert.Equal("https://example.org/img.jpg", jsonLd["image"]);
        Assert.Equal("2026-01-01T10:00:00Z", jsonLd["datePublished"]);
        Assert.Equal("2026-01-02T11:00:00Z", jsonLd["dateModified"]);
        var mainEntityOfPage = Assert.IsType<Dictionary<string, object?>>(jsonLd["mainEntityOfPage"]);
        Assert.Equal("https://example.org/haberler/x", mainEntityOfPage["@id"]);
        var publisher = Assert.IsType<Dictionary<string, object?>>(jsonLd["publisher"]);
        Assert.Equal("Gençlik Merkezi", publisher["name"]);
        var logo = Assert.IsType<Dictionary<string, object?>>(publisher["logo"]);
        Assert.Equal("https://example.org/logo.png", logo["url"]);
    }

    [Fact]
    public void BuildArticle_UsesArticleTypeForArticleSchemaKind()
    {
        var jsonLd = StructuredDataBuilder.BuildArticle(
            ContentSchemaKind.Article, "Başlık", "Açıklama", null, DateTime.UtcNow, null, "https://example.org/projeler/x", "Site", null);

        Assert.Equal("Article", jsonLd["@type"]);
    }

    [Fact]
    public void BuildArticle_WithoutImageOrDateModifiedOrPublisherLogo_OmitsThoseKeys()
    {
        var jsonLd = StructuredDataBuilder.BuildArticle(
            ContentSchemaKind.Article, "Başlık", "Açıklama", null, DateTime.UtcNow, null, "https://example.org/projeler/x", "Site", null);

        Assert.False(jsonLd.ContainsKey("image"));
        Assert.False(jsonLd.ContainsKey("dateModified"));
        var publisher = Assert.IsType<Dictionary<string, object?>>(jsonLd["publisher"]);
        Assert.False(publisher.ContainsKey("logo"));
        // ADR-024 §15 "author verisi yoksa yazılmaz": this project's ContentItem has no author data at
        // all, so BuildArticle never has a parameter for it and never produces the key.
        Assert.False(jsonLd.ContainsKey("author"));
    }

    [Theory]
    [InlineData(EventFormat.InPerson, "https://schema.org/OfflineEventAttendanceMode")]
    [InlineData(EventFormat.Online, "https://schema.org/OnlineEventAttendanceMode")]
    [InlineData(EventFormat.Hybrid, "https://schema.org/MixedEventAttendanceMode")]
    public void BuildEvent_MapsFormatToAttendanceMode(EventFormat format, string expectedAttendanceMode)
    {
        var jsonLd = StructuredDataBuilder.BuildEvent(
            "Etkinlik", DateTime.UtcNow, DateTime.UtcNow.AddHours(2), isCancelled: false, format, "Salon", "Adres", "Açıklama", null,
            "https://example.org/etkinlikler/x");

        Assert.Equal(expectedAttendanceMode, jsonLd["eventAttendanceMode"]);
    }

    [Theory]
    [InlineData(EventFormat.InPerson)]
    [InlineData(EventFormat.Hybrid)]
    public void BuildEvent_InPersonOrHybrid_UsesPlaceLocationWithVenueNameAndAddress(EventFormat format)
    {
        var jsonLd = StructuredDataBuilder.BuildEvent(
            "Etkinlik", DateTime.UtcNow, DateTime.UtcNow.AddHours(2), isCancelled: false, format, "Salon", "Adres No:1", "Açıklama", null,
            "https://example.org/etkinlikler/x");

        var location = Assert.IsType<Dictionary<string, object?>>(jsonLd["location"]);
        Assert.Equal("Place", location["@type"]);
        Assert.Equal("Salon", location["name"]);
        Assert.Equal("Adres No:1", location["address"]);
    }

    [Fact]
    public void BuildEvent_OnlineOnly_UsesVirtualLocationWithOwnPageUrlNotOnlineLink()
    {
        var jsonLd = StructuredDataBuilder.BuildEvent(
            "Etkinlik", DateTime.UtcNow, DateTime.UtcNow.AddHours(2), isCancelled: false, EventFormat.Online, string.Empty, string.Empty,
            "Açıklama", null, "https://example.org/etkinlikler/x");

        var location = Assert.IsType<Dictionary<string, object?>>(jsonLd["location"]);
        Assert.Equal("VirtualLocation", location["@type"]);
        Assert.Equal("https://example.org/etkinlikler/x", location["url"]);
    }

    [Fact]
    public void BuildEvent_NeverContainsOnlineLinkKeyAnywhere()
    {
        var jsonLd = StructuredDataBuilder.BuildEvent(
            "Etkinlik", DateTime.UtcNow, DateTime.UtcNow.AddHours(2), isCancelled: false, EventFormat.Hybrid, "Salon", "Adres", "Açıklama", null,
            "https://example.org/etkinlikler/x");

        // Regression guard (ADR-024 §11.3/§15): no key in the whole object graph may be/contain
        // "onlineLink" - BuildEvent takes no such parameter at all, but this also catches a future
        // accidental addition.
        Assert.DoesNotContain(jsonLd.Keys, k => k.Contains("onlineLink", StringComparison.OrdinalIgnoreCase));
        var location = Assert.IsType<Dictionary<string, object?>>(jsonLd["location"]);
        Assert.DoesNotContain(location.Keys, k => k.Contains("onlineLink", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(false, "https://schema.org/EventScheduled")]
    [InlineData(true, "https://schema.org/EventCancelled")]
    public void BuildEvent_MapsIsCancelledToEventStatus(bool isCancelled, string expectedStatus)
    {
        var jsonLd = StructuredDataBuilder.BuildEvent(
            "Etkinlik", DateTime.UtcNow, DateTime.UtcNow.AddHours(2), isCancelled, EventFormat.InPerson, "Salon", "Adres", "Açıklama", null,
            "https://example.org/etkinlikler/x");

        Assert.Equal(expectedStatus, jsonLd["eventStatus"]);
    }

    [Fact]
    public void BuildFaqPage_ConvertsAnswerHtmlToPlainText()
    {
        var jsonLd = StructuredDataBuilder.BuildFaqPage([("Soru 1?", "<p>Cevap <strong>metni</strong>.</p>")]);

        Assert.Equal("FAQPage", jsonLd["@type"]);
        var mainEntity = Assert.IsAssignableFrom<IReadOnlyList<Dictionary<string, object?>>>(jsonLd["mainEntity"]);
        var question = Assert.Single(mainEntity);
        Assert.Equal("Question", question["@type"]);
        Assert.Equal("Soru 1?", question["name"]);
        var acceptedAnswer = Assert.IsType<Dictionary<string, object?>>(question["acceptedAnswer"]);
        Assert.Equal("Answer", acceptedAnswer["@type"]);
        Assert.Equal("Cevap metni .", acceptedAnswer["text"]);
    }

    [Fact]
    public void BuildOrganization_WithOnlyNameAndUrl_OmitsEveryOptionalField()
    {
        var jsonLd = StructuredDataBuilder.BuildOrganization("Gençlik Merkezi", "https://example.org", null, [], null, null, null);

        Assert.Equal("Organization", jsonLd["@type"]);
        Assert.Equal("Gençlik Merkezi", jsonLd["name"]);
        Assert.Equal("https://example.org", jsonLd["url"]);
        Assert.False(jsonLd.ContainsKey("logo"));
        Assert.False(jsonLd.ContainsKey("sameAs"));
        Assert.False(jsonLd.ContainsKey("contactPoint"));
        Assert.False(jsonLd.ContainsKey("address"));
    }

    [Fact]
    public void BuildOrganization_WithEveryFieldPresent_IncludesAllOfThem()
    {
        var jsonLd = StructuredDataBuilder.BuildOrganization(
            "Gençlik Merkezi", "https://example.org", "https://example.org/logo.png", ["https://instagram.com/x", "https://twitter.com/x"],
            "+90 555 000 00 00", "info@example.org", "Örnek Mah. No:1, İstanbul");

        Assert.Equal("https://example.org/logo.png", jsonLd["logo"]);
        var sameAs = Assert.IsAssignableFrom<IReadOnlyList<string>>(jsonLd["sameAs"]);
        Assert.Equal(2, sameAs.Count);
        var contactPoint = Assert.IsType<Dictionary<string, object?>>(jsonLd["contactPoint"]);
        Assert.Equal("+90 555 000 00 00", contactPoint["telephone"]);
        Assert.Equal("info@example.org", contactPoint["email"]);
        Assert.Equal("Örnek Mah. No:1, İstanbul", jsonLd["address"]);
    }

    [Fact]
    public void BuildOrganization_WithOnlyPhoneNoEmail_ContactPointOmitsEmailKey()
    {
        var jsonLd = StructuredDataBuilder.BuildOrganization("Gençlik Merkezi", "https://example.org", null, [], "+90 555 000 00 00", null, null);

        var contactPoint = Assert.IsType<Dictionary<string, object?>>(jsonLd["contactPoint"]);
        Assert.True(contactPoint.ContainsKey("telephone"));
        Assert.False(contactPoint.ContainsKey("email"));
    }
}
