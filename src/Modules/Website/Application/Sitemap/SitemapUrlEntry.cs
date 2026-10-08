using GenclikMerkezi.Modules.Website.Application.RouteResolution;

namespace GenclikMerkezi.Modules.Website.Application.Sitemap;

// ADR-024 §15 (Faz 5 Görev 5): one <url> entry - Loc is the site-relative public path (leading slash,
// no language/host prefix added yet; SitemapContentCollector's caller prefixes Website:PublicSiteBaseUrl
// once, at render time). Alternates already includes Loc's own language (self-referencing hreflang,
// the convention search engines expect) and "x-default" when the default language has a translation.
public sealed record SitemapUrlEntry(string Loc, DateTime? LastModUtc, IReadOnlyList<RouteAlternate> Alternates);
