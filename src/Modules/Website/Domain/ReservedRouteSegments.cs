namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §15 (Faz 1a Görev 2, also used by Görev 4's root-level ContentItem path checks): a
// ContentType's RoutePrefix, and a root-level ContentItem's first path segment, can never collide
// with these static infrastructure routes or with any SiteLanguage code. The language-code half of
// the check is data, not a compile-time constant, so it is a parameter here rather than baked in -
// this stays a pure Domain helper with no repository access of its own.
public static class ReservedRouteSegments
{
    // "giris"/"uye-ol"/"ilanlar"/"newsletter" (Faz 3 Koşu 1 bulgusu, FRONTEND-FAZ3-MASTER-PROMPT.md +
    // BLOK-ESLEMESI.md §(iv)(g)) are apps/web's own hardcoded system pages, not Website ContentType
    // routes - giris/uye-ol (login/register landing), ilanlar (Employer-owned job listing/detail,
    // outside this module entirely), newsletter (the literal segment the backend's own confirmation
    // email link already hardcodes: {PublicSiteBaseUrl}/newsletter/confirm). Deliberately NOT added
    // here: "events"/"etkinlikler" - the seeded Event ContentType already owns both as its own
    // RoutePrefix (en/tr), so reserving either would break RoutePrefixGuard on that existing data; see
    // the Faz 3 Koşu 1 report for why the frontend's /events/registrations/verify|cancel pages still
    // don't collide with it in practice (Next.js's static route always wins over the catch-all, and
    // ResolveRoute's Listing match requires an exact single-segment path).
    public static readonly IReadOnlyCollection<string> StaticSegments = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "api", "admin", "portal", "webuploads", "uploads", "media", "assets", "static", "sitemap.xml", "robots.txt",
        "giris", "uye-ol", "ilanlar", "newsletter",
    };

    // languageCodes is every SiteLanguage code, active or not - an inactive language can be
    // reactivated later, and a ContentType's RoutePrefix must never collide with it either.
    public static bool IsReserved(string segment, IEnumerable<string> languageCodes) =>
        StaticSegments.Contains(segment) || languageCodes.Contains(segment, StringComparer.OrdinalIgnoreCase);
}
