namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-017 Decision 3 pattern (mirrors ReferenceDataCacheKeys): every key for the public site
// bootstrap response is built under the same InvalidationPrefix, so one
// ICacheService.RemoveByPrefix call clears every language's cached entry at once.
public static class WebsiteCacheKeys
{
    private const string Prefix = "website:public-site:";

    public static string PublicSite(string languageCode) => $"{Prefix}{languageCode}";

    public static string InvalidationPrefix => Prefix;

    // ADR-024 §17 (Faz 1b Görev 7): everything derived from content/type/category/tag/video/media/
    // redirect/language state - the public list, the public detail, and public route resolution - is
    // cached under this single prefix, so one coarse-grained RemoveByPrefix (deliberately not
    // fine-grained per ADR-024 Faz 1b's own "kaba taneli temizlik kabul edilebilir" call) clears all
    // of it whenever any of that state changes.
    private const string PublicContentPrefix = "website:public-content:";

    public static string PublicContentList(
        string contentTypeKey, string languageCode, string? category, string? tag, string? from, string? to, bool? featured, int page,
        int pageSize) =>
        $"{PublicContentPrefix}list:{contentTypeKey}:{languageCode}:{category}:{tag}:{from}:{to}:{featured}:{page}:{pageSize}";

    public static string PublicContentDetail(Guid contentItemId, string languageCode) =>
        $"{PublicContentPrefix}detail:{contentItemId}:{languageCode}";

    public static string RouteResolution(string normalizedPath) => $"{PublicContentPrefix}route:{normalizedPath}";

    // Faz 2 Görev 5 master prompt §5.1/§5.3: the public home page's resolved blocks - lives under the
    // same PublicContentPrefix as the list/detail/route caches, so it is already cleared by every
    // mutation handler that calls InvalidatePublicContent/InvalidateAllPublic (content, content type,
    // category, slider, partner, impact metric and page layout handlers all already do).
    public static string PublicHome(string languageCode) => $"{PublicContentPrefix}home:{languageCode}";

    public static string PublicContentInvalidationPrefix => PublicContentPrefix;

    // ADR-024 §12.1 (Faz 3 Görev 2): the effective-version and specific-version public responses live
    // under the same PublicContentPrefix as list/detail/route/home, so the existing
    // InvalidatePublicContent/InvalidateAllPublic sweep every LegalDocument-mutating handler already
    // calls (§1 "Cache: Public okumaları etkileyen her mutasyon InvalidateAllPublic çağırır") clears
    // these too, with no new invalidation method needed.
    public static string PublicLegalDocument(string key, string languageCode) =>
        $"{PublicContentPrefix}legal-document:{key}:{languageCode}";

    public static string PublicLegalDocumentVersion(string key, int versionNumber, string languageCode) =>
        $"{PublicContentPrefix}legal-document-version:{key}:{versionNumber}:{languageCode}";

    public static string PublicForm(string key, string languageCode) => $"{PublicContentPrefix}form:{key}:{languageCode}";
}
