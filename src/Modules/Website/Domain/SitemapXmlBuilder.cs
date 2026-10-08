using System.Text;
using System.Xml.Linq;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §15 (Faz 5 Görev 5): pure XML text assembly, the same Domain placement
// IcsEventCalendarBuilder already uses for its own pure text format - no repository access, every
// input already resolved by the caller. XElement/XAttribute escape text content and attribute values
// automatically (&, <, > and quotes), so callers never need to escape loc/hreflang values themselves.
public static class SitemapXmlBuilder
{
    private static readonly XNamespace SitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
    private static readonly XNamespace XhtmlNamespace = "http://www.w3.org/1999/xhtml";

    // entries' Loc values are already absolute (the caller prefixes Website:PublicSiteBaseUrl before
    // calling this).
    public static string BuildUrlSet(IReadOnlyList<(string Loc, DateTime? LastModUtc, IReadOnlyList<(string HrefLang, string Href)> Alternates)> entries)
    {
        var root = new XElement(
            SitemapNamespace + "urlset",
            new XAttribute(XNamespace.Xmlns + "xhtml", XhtmlNamespace),
            entries.Select(BuildUrlElement));

        return Serialize(root);
    }

    // sitemapLocs are already absolute sitemap-segment URLs (e.g. ".../sitemap-1.xml").
    public static string BuildIndex(IReadOnlyList<string> sitemapLocs)
    {
        var root = new XElement(
            SitemapNamespace + "sitemapindex",
            sitemapLocs.Select(loc => new XElement(SitemapNamespace + "sitemap", new XElement(SitemapNamespace + "loc", loc))));

        return Serialize(root);
    }

    private static XElement BuildUrlElement(
        (string Loc, DateTime? LastModUtc, IReadOnlyList<(string HrefLang, string Href)> Alternates) entry)
    {
        var url = new XElement(SitemapNamespace + "loc", entry.Loc);
        var element = new XElement(SitemapNamespace + "url", url);

        if (entry.LastModUtc is { } lastMod)
        {
            element.Add(new XElement(SitemapNamespace + "lastmod", lastMod.ToString("yyyy-MM-ddTHH:mm:ssZ")));
        }

        foreach (var (hrefLang, href) in entry.Alternates)
        {
            element.Add(new XElement(
                XhtmlNamespace + "link",
                new XAttribute("rel", "alternate"),
                new XAttribute("hreflang", hrefLang),
                new XAttribute("href", href)));
        }

        return element;
    }

    private static string Serialize(XElement root)
    {
        var document = new XDocument(new XDeclaration("1.0", "UTF-8", null), root);
        using var writer = new Utf8StringWriter();
        document.Save(writer);
        return writer.ToString();
    }

    private sealed class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
