namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §15 (Faz 1a Görev 2, also used by Görev 4's root-level ContentItem path checks): a
// ContentType's RoutePrefix, and a root-level ContentItem's first path segment, can never collide
// with these static infrastructure routes or with any SiteLanguage code. The language-code half of
// the check is data, not a compile-time constant, so it is a parameter here rather than baked in -
// this stays a pure Domain helper with no repository access of its own.
public static class ReservedRouteSegments
{
    public static readonly IReadOnlyCollection<string> StaticSegments = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "api", "admin", "portal", "webuploads", "uploads", "media", "assets", "static", "sitemap.xml", "robots.txt",
    };

    // languageCodes is every SiteLanguage code, active or not - an inactive language can be
    // reactivated later, and a ContentType's RoutePrefix must never collide with it either.
    public static bool IsReserved(string segment, IEnumerable<string> languageCodes) =>
        StaticSegments.Contains(segment) || languageCodes.Contains(segment, StringComparer.OrdinalIgnoreCase);
}
