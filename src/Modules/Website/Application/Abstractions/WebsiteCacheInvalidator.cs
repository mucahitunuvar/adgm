using GenclikMerkezi.SharedKernel.Abstractions;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// Called by every SiteSettings- and SiteLanguage-mutating command handler (ADR-024 §13/§3): the
// public site bootstrap response embeds both the active language list and SiteSettings' fields in
// one cached blob per resolved language, so a change to either source can leave any of those blobs
// stale. ICacheService is a plain SharedKernel abstraction, not module Infrastructure.
public static class WebsiteCacheInvalidator
{
    public static void InvalidatePublicSite(ICacheService cacheService) =>
        cacheService.RemoveByPrefix(WebsiteCacheKeys.InvalidationPrefix);

    // ADR-024 §17 (Faz 1b Görev 7): called, after commit, by every command handler that mutates a
    // ContentItem, ContentType, ContentCategory, ContentTag, Video, MediaAsset, Redirect or
    // SiteLanguage aggregate - anything the public list/detail/route-resolution responses could have
    // read. A coarse-grained sweep of the whole public-content prefix, not per-key targeting
    // (ADR-024 Faz 1b: "kaba taneli temizlik kabul edilebilir").
    public static void InvalidatePublicContent(ICacheService cacheService) =>
        cacheService.RemoveByPrefix(WebsiteCacheKeys.PublicContentInvalidationPrefix);
}
