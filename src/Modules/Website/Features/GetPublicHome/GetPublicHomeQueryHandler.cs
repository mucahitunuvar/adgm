using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.BlockTypes.PublicResolution;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicHome;

// Faz 2 Görev 5 master prompt §5.1/§5.3: the home page's published blocks plus the site's default SEO
// ("ana sayfa için başlık site adıdır" - the same ContentSeoResolver chain every other public handler
// shares, with no stored SeoMetadata of its own to start from). TTL is shortened to the site-wide
// earliest upcoming content/slide transition, the same calculation GetPublicSiteQueryHandler already
// uses (§5.3 "Pratikte Görev 1'deki site geneli en yakın zamanlama hesabını kullanmak yeterlidir").
public sealed class GetPublicHomeQueryHandler(
    ISiteLanguageRepository siteLanguageRepository,
    IPageLayoutRepository pageLayoutRepository,
    PublicPageLayoutResolver publicPageLayoutResolver,
    ISiteSettingsRepository siteSettingsRepository,
    IMediaAssetRepository mediaAssetRepository,
    IFileStorageService fileStorageService,
    IContentItemRepository contentItemRepository,
    ISliderRepository sliderRepository,
    ICacheService cacheService,
    TimeProvider timeProvider)
    : IRequestHandler<GetPublicHomeQuery, Result<PublicHomeResponse>>
{
    public async Task<Result<PublicHomeResponse>> Handle(GetPublicHomeQuery request, CancellationToken cancellationToken)
    {
        var activeLanguages = await siteLanguageRepository.GetActiveAsync(cancellationToken);
        var defaultLanguage = activeLanguages.First(l => l.IsDefault);
        var resolvedLanguage = (!string.IsNullOrWhiteSpace(request.Lang)
            ? activeLanguages.FirstOrDefault(l => string.Equals(l.Code.Value, request.Lang, StringComparison.OrdinalIgnoreCase))
            : null) ?? defaultLanguage;

        var now = timeProvider.GetUtcNow().UtcDateTime;

        var earliestUpcomingContentTransition = await contentItemRepository.GetEarliestUpcomingTransitionAsync(now, cancellationToken);
        var earliestUpcomingSlideTransition = await sliderRepository.GetEarliestUpcomingSlideTransitionAsync(now, cancellationToken);
        var ttl = ContentCacheTtlCalculator.Calculate(now, [earliestUpcomingContentTransition, earliestUpcomingSlideTransition]);

        var response = await cacheService.GetOrCreateAsync(
            WebsiteCacheKeys.PublicHome(resolvedLanguage.Code.Value),
            ct => BuildResponseAsync(resolvedLanguage.Code, defaultLanguage.Code, now, ct),
            ttl,
            cancellationToken);

        return Result.Success(response);
    }

    private async Task<PublicHomeResponse> BuildResponseAsync(
        LanguageCode languageCode, LanguageCode defaultLanguageCode, DateTime now, CancellationToken cancellationToken)
    {
        var layout = await pageLayoutRepository.GetHomeAsync(cancellationToken)
            ?? throw new InvalidOperationException("The home page layout is missing its seeded row.");

        var blocks = await publicPageLayoutResolver.ResolveAsync(
            layout.PublishedBlocks, languageCode, defaultLanguageCode, now, cancellationToken);

        var settings = await siteSettingsRepository.GetAsync(cancellationToken) ?? SiteSettings.CreateDefault();
        var settingsTranslation = settings.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        var homePath = RoutePathFormat.BuildPublicPath(languageCode.Value, defaultLanguageCode.Value, string.Empty);

        var resolvedSeo = ContentSeoResolver.Resolve(
            SeoMetadata.CreateEmpty(), settingsTranslation?.SiteName ?? string.Empty, string.Empty, homePath, null, null,
            settings.DefaultOgImageMediaId, settingsTranslation?.DefaultMetaDescription ?? string.Empty);
        var ogImage = await BuildOgImageUrlAsync(resolvedSeo.OgImageMediaId, cancellationToken);
        var seo = new PublicHomeSeoResponse(
            resolvedSeo.MetaTitle, resolvedSeo.MetaDescription, resolvedSeo.OgTitle, resolvedSeo.OgDescription, ogImage, resolvedSeo.CanonicalUrl,
            resolvedSeo.NoIndex);

        return new PublicHomeResponse(blocks, seo);
    }

    private async Task<string?> BuildOgImageUrlAsync(Guid? mediaAssetId, CancellationToken cancellationToken)
    {
        if (mediaAssetId is not { } id)
        {
            return null;
        }

        var mediaAsset = await mediaAssetRepository.GetByIdAsync(id, cancellationToken);
        return mediaAsset is null ? null : await fileStorageService.GetUrlAsync(mediaAsset.Original.FileKey, cancellationToken);
    }
}
