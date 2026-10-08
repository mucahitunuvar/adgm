using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.Sitemap;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace GenclikMerkezi.Modules.Website.Features.GetSitemap;

// ADR-024 §15 (Faz 5 Görev 5): GET /sitemap.xml (root path, anonymous). SiteSettings.
// AllowSearchEngineIndexing=false returns an empty <urlset> rather than 404 ("Kapalıyken sitemap boş
// döner") - there is no wrong state for a crawler to see, just nothing to index. Entries are cached for
// 1 hour under WebsiteCacheKeys.PublicSitemapEntries (cleared by InvalidateAllPublic like every other
// public-content cache entry); GetSitemapSegmentQueryHandler reuses the exact same cached list, so a
// sitemap split across many segments never re-runs the underlying ContentItem/SearchDocument walk per
// segment request - only the cheap XML-rendering/slicing step repeats.
public sealed class GetSitemapQueryHandler(
    ISiteSettingsRepository siteSettingsRepository,
    SitemapContentCollector sitemapContentCollector,
    ICacheService cacheService,
    IConfiguration configuration)
    : IRequestHandler<GetSitemapQuery, Result<string>>
{
    public async Task<Result<string>> Handle(GetSitemapQuery request, CancellationToken cancellationToken)
    {
        var settings = await siteSettingsRepository.GetAsync(cancellationToken) ?? SiteSettings.CreateDefault();
        if (!settings.AllowSearchEngineIndexing)
        {
            return Result.Success(SitemapXmlBuilder.BuildUrlSet([]));
        }

        var entries = await cacheService.GetOrCreateAsync(
            WebsiteCacheKeys.PublicSitemapEntries, sitemapContentCollector.CollectAsync, TimeSpan.FromHours(1), cancellationToken);

        var publicSiteBaseUrl = (configuration["Website:PublicSiteBaseUrl"] ?? string.Empty).TrimEnd('/');
        var maxUrlsPerFile = configuration.GetValue(
            "Website:Sitemap:MaxUrlsPerSitemapFile", SitemapEntryXmlMapper.DefaultMaxUrlsPerSitemapFile);

        if (entries.Count <= maxUrlsPerFile)
        {
            return Result.Success(SitemapXmlBuilder.BuildUrlSet(SitemapEntryXmlMapper.ToXmlEntries(entries, publicSiteBaseUrl)));
        }

        var segmentCount = (int)Math.Ceiling(entries.Count / (double)maxUrlsPerFile);
        var sitemapLocs = Enumerable.Range(1, segmentCount).Select(n => $"{publicSiteBaseUrl}/sitemap-{n}.xml").ToList();

        return Result.Success(SitemapXmlBuilder.BuildIndex(sitemapLocs));
    }
}
