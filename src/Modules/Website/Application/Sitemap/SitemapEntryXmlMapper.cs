namespace GenclikMerkezi.Modules.Website.Application.Sitemap;

// ADR-024 §15 (Faz 5 Görev 5): prefixes every SitemapUrlEntry's site-relative Loc/alternate paths with
// Website:PublicSiteBaseUrl and reshapes them into the plain tuples SitemapXmlBuilder (Domain, which
// cannot reference this Application-layer record) accepts - shared by GetSitemapQueryHandler and
// GetSitemapSegmentQueryHandler so neither vertical slice duplicates the same projection, and by
// GetSitemapQueryHandler/GetSitemapSegmentQueryHandler for the sitemap-splitting threshold itself.
public static class SitemapEntryXmlMapper
{
    // ADR-024 §15: "sitemap index olur, her biri en çok 10.000 URL" - comfortably under the sitemaps.org
    // protocol's own 50,000 URL / 50MB-per-file limit. Configurable via Website:Sitemap:
    // MaxUrlsPerSitemapFile so a test can exercise the splitting logic without generating 10,000+ rows.
    public const int DefaultMaxUrlsPerSitemapFile = 10_000;

    public static IReadOnlyList<(string Loc, DateTime? LastModUtc, IReadOnlyList<(string HrefLang, string Href)> Alternates)> ToXmlEntries(
        IReadOnlyList<SitemapUrlEntry> entries, string publicSiteBaseUrl) =>
        entries
            .Select(e => (
                Loc: $"{publicSiteBaseUrl}{e.Loc}",
                e.LastModUtc,
                Alternates: (IReadOnlyList<(string HrefLang, string Href)>)e.Alternates
                    .Select(a => (a.LanguageCode, Href: $"{publicSiteBaseUrl}{a.Path}"))
                    .ToList()))
            .ToList();
}
