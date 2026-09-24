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
}
