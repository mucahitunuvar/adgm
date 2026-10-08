using System.Xml.Linq;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

// ADR-024 §15 (Faz 5 Görev 5): format-level coverage for the sitemap/sitemap-index XML - escaping,
// lastmod formatting, hreflang alternates and the sitemapindex shape. Parses the output back with
// XDocument rather than string-matching, so a passing test also proves the XML is well-formed.
public class SitemapXmlBuilderTests
{
    private static readonly XNamespace SitemapNs = "http://www.sitemaps.org/schemas/sitemap/0.9";
    private static readonly XNamespace XhtmlNs = "http://www.w3.org/1999/xhtml";

    [Fact]
    public void BuildUrlSet_EscapesAmpersandAndAngleBracketsInLoc()
    {
        var xml = SitemapXmlBuilder.BuildUrlSet(
            [("https://example.com/haberler/a&b<c>", (DateTime?)null, Array.Empty<(string, string)>())]);

        Assert.Contains("a&amp;b&lt;c&gt;", xml, StringComparison.Ordinal);

        // Still parses as well-formed XML, and the round-tripped value is the original, unescaped text.
        var document = XDocument.Parse(xml);
        var loc = document.Root!.Element(SitemapNs + "url")!.Element(SitemapNs + "loc")!.Value;
        Assert.Equal("https://example.com/haberler/a&b<c>", loc);
    }

    [Fact]
    public void BuildUrlSet_IncludesXmlDeclarationWithUtf8Encoding()
    {
        var xml = SitemapXmlBuilder.BuildUrlSet([]);

        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"", xml, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildUrlSet_OmitsLastmodWhenNull()
    {
        var xml = SitemapXmlBuilder.BuildUrlSet([("https://example.com/", (DateTime?)null, Array.Empty<(string, string)>())]);

        var document = XDocument.Parse(xml);
        var urlElement = document.Root!.Element(SitemapNs + "url")!;
        Assert.Null(urlElement.Element(SitemapNs + "lastmod"));
    }

    [Fact]
    public void BuildUrlSet_FormatsLastmodAsIso8601Utc()
    {
        var lastMod = new DateTime(2026, 3, 5, 10, 15, 0, DateTimeKind.Utc);
        var xml = SitemapXmlBuilder.BuildUrlSet([("https://example.com/", (DateTime?)lastMod, Array.Empty<(string, string)>())]);

        var document = XDocument.Parse(xml);
        var lastmod = document.Root!.Element(SitemapNs + "url")!.Element(SitemapNs + "lastmod")!.Value;
        Assert.Equal("2026-03-05T10:15:00Z", lastmod);
    }

    [Fact]
    public void BuildUrlSet_WritesOneAlternateLinkPerEntry()
    {
        var xml = SitemapXmlBuilder.BuildUrlSet(
            [
                ("https://example.com/haberler/x", (DateTime?)null,
                    new[] { ("tr", "https://example.com/haberler/x"), ("en", "https://example.com/en/news/x"), ("x-default", "https://example.com/haberler/x") }),
            ]);

        var document = XDocument.Parse(xml);
        var links = document.Root!.Element(SitemapNs + "url")!.Elements(XhtmlNs + "link").ToList();

        Assert.Equal(3, links.Count);
        Assert.Contains(links, l => l.Attribute("hreflang")!.Value == "en" && l.Attribute("href")!.Value == "https://example.com/en/news/x");
        Assert.Contains(links, l => l.Attribute("hreflang")!.Value == "x-default");
        Assert.All(links, l => Assert.Equal("alternate", l.Attribute("rel")!.Value));
    }

    [Fact]
    public void BuildIndex_WritesOneSitemapElementPerLoc()
    {
        var xml = SitemapXmlBuilder.BuildIndex(["https://example.com/sitemap-1.xml", "https://example.com/sitemap-2.xml"]);

        var document = XDocument.Parse(xml);
        var locs = document.Root!.Elements(SitemapNs + "sitemap").Select(e => e.Element(SitemapNs + "loc")!.Value).ToList();

        Assert.Equal(["https://example.com/sitemap-1.xml", "https://example.com/sitemap-2.xml"], locs);
    }
}
