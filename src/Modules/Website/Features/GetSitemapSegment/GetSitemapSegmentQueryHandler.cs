using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.Sitemap;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace GenclikMerkezi.Modules.Website.Features.GetSitemapSegment;

// ADR-024 §15 (Faz 5 Görev 5): GET /sitemap-{n}.xml - only ever a valid request when /sitemap.xml
// itself is currently a <sitemapindex> (entries.Count > maxUrlsPerFile) and n falls within that index's
// segment count; otherwise 404, the same "no sitemap index was ever offered at this URL" response a
// crawler should get. Reuses the exact same cached entry list GetSitemapQueryHandler populates - no
// separate cache entry per segment.
public sealed class GetSitemapSegmentQueryHandler(
    ISiteSettingsRepository siteSettingsRepository,
    SitemapContentCollector sitemapContentCollector,
    ICacheService cacheService,
    IConfiguration configuration)
    : IRequestHandler<GetSitemapSegmentQuery, Result<string>>
{
    private static readonly Error NotFoundError = Error.NotFound("Sitemap.SegmentNotFound", "This sitemap segment could not be found.");

    public async Task<Result<string>> Handle(GetSitemapSegmentQuery request, CancellationToken cancellationToken)
    {
        if (request.Segment < 1)
        {
            return Result.Failure<string>(NotFoundError);
        }

        var settings = await siteSettingsRepository.GetAsync(cancellationToken) ?? SiteSettings.CreateDefault();
        if (!settings.AllowSearchEngineIndexing)
        {
            return Result.Failure<string>(NotFoundError);
        }

        var entries = await cacheService.GetOrCreateAsync(
            WebsiteCacheKeys.PublicSitemapEntries, sitemapContentCollector.CollectAsync, TimeSpan.FromHours(1), cancellationToken);

        var maxUrlsPerFile = configuration.GetValue(
            "Website:Sitemap:MaxUrlsPerSitemapFile", SitemapEntryXmlMapper.DefaultMaxUrlsPerSitemapFile);

        // No index was ever offered at /sitemap.xml when everything fits in one file - every segment
        // URL is then a 404, not an alternate way to read the same (single) sitemap.
        if (entries.Count <= maxUrlsPerFile)
        {
            return Result.Failure<string>(NotFoundError);
        }

        var segmentCount = (int)Math.Ceiling(entries.Count / (double)maxUrlsPerFile);
        if (request.Segment > segmentCount)
        {
            return Result.Failure<string>(NotFoundError);
        }

        var publicSiteBaseUrl = (configuration["Website:PublicSiteBaseUrl"] ?? string.Empty).TrimEnd('/');
        var segmentEntries = entries.Skip((request.Segment - 1) * maxUrlsPerFile).Take(maxUrlsPerFile).ToList();

        return Result.Success(SitemapXmlBuilder.BuildUrlSet(SitemapEntryXmlMapper.ToXmlEntries(segmentEntries, publicSiteBaseUrl)));
    }
}
