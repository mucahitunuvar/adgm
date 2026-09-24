namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-017 Decision 3 pattern (mirrors ReferenceDataCacheKeys): every key for the public site
// bootstrap response is built under the same InvalidationPrefix, so one
// ICacheService.RemoveByPrefix call clears every language's cached entry at once.
public static class WebsiteCacheKeys
{
    private const string Prefix = "website:public-site:";

    public static string PublicSite(string languageCode) => $"{Prefix}{languageCode}";

    public static string InvalidationPrefix => Prefix;
}
