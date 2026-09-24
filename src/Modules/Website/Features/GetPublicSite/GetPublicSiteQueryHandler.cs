using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicSite;

// ADR-024 §13/§15: unauthenticated - the frontend calls this once at bootstrap, before any
// page-specific content, to render the site chrome (theme, contact, footer) in the visitor's
// language. Cached per resolved language (ADR-017); invalidated by every SiteSettings- and
// SiteLanguage-mutating command (WebsiteCacheInvalidator).
public sealed class GetPublicSiteQueryHandler(
    ISiteSettingsRepository siteSettingsRepository,
    ISiteLanguageRepository siteLanguageRepository,
    IMediaAssetRepository mediaAssetRepository,
    IFileStorageService fileStorageService,
    ICacheService cacheService)
    : IRequestHandler<GetPublicSiteQuery, Result<PublicSiteResponse>>
{
    public async Task<Result<PublicSiteResponse>> Handle(GetPublicSiteQuery request, CancellationToken cancellationToken)
    {
        var activeLanguages = await siteLanguageRepository.GetActiveAsync(cancellationToken);

        // Invariant guaranteed by SiteLanguage's own domain rules (Görev 2): the default language
        // can never be deactivated, so there is always exactly one active default to fall back to.
        var resolvedLanguage = (!string.IsNullOrWhiteSpace(request.Lang)
            ? activeLanguages.FirstOrDefault(l => string.Equals(l.Code.Value, request.Lang, StringComparison.OrdinalIgnoreCase))
            : null) ?? activeLanguages.First(l => l.IsDefault);

        var response = await cacheService.GetOrCreateAsync(
            WebsiteCacheKeys.PublicSite(resolvedLanguage.Code.Value),
            async ct => await BuildResponseAsync(activeLanguages, resolvedLanguage, ct),
            cancellationToken: cancellationToken);

        return Result.Success(response);
    }

    private async Task<PublicSiteResponse> BuildResponseAsync(
        IReadOnlyList<SiteLanguage> activeLanguages, SiteLanguage resolvedLanguage, CancellationToken cancellationToken)
    {
        var settings = await siteSettingsRepository.GetAsync(cancellationToken) ?? SiteSettings.CreateDefault();

        var languageResponses = activeLanguages
            .OrderBy(l => l.SortOrder)
            .Select(l => new PublicSiteLanguageResponse(l.Code.Value, l.Name, l.IsDefault, l.SortOrder))
            .ToList();

        var themeResponse = new PublicSiteThemeResponse(settings.Theme.PrimaryColorHex, settings.Theme.SecondaryColorHex, settings.Theme.FontFamily);

        var contactResponse = new PublicContactInfoResponse(
            settings.Contact.Address, settings.Contact.Phone, settings.Contact.Email, settings.Contact.WhatsApp, settings.Contact.MapEmbedUrl);

        var socialLinks = settings.SocialLinks
            .OrderBy(l => l.SortOrder)
            .Select(l => new PublicSocialLinkResponse(l.Platform, l.Url, l.SortOrder))
            .ToList();

        // ADR-024 §13: bank accounts only make sense once the (currently inactive) donation page is
        // enabled - publishing account numbers with no page to explain their purpose would be a
        // pointless disclosure, not a feature.
        var bankAccounts = settings.DonationPageEnabled
            ? settings.BankAccounts
                .Where(a => a.IsActive)
                .OrderBy(a => a.SortOrder)
                .Select(a => new PublicBankAccountResponse(a.Iban.Value, a.BankName, a.AccountHolder, a.Currency.ToString(), a.Description, a.SortOrder))
                .ToList()
            : [];

        var translation = settings.Translations.FirstOrDefault(t => t.LanguageCode == resolvedLanguage.Code);

        return new PublicSiteResponse(
            languageResponses, resolvedLanguage.Code.Value,
            await ResolveMediaUrlAsync(settings.LogoLightMediaAssetId, cancellationToken),
            await ResolveMediaUrlAsync(settings.LogoDarkMediaAssetId, cancellationToken),
            await ResolveMediaUrlAsync(settings.FaviconMediaAssetId, cancellationToken),
            await ResolveMediaUrlAsync(settings.DefaultOgImageMediaId, cancellationToken),
            themeResponse, contactResponse, socialLinks, bankAccounts,
            translation?.SiteName ?? string.Empty, translation?.Tagline ?? string.Empty,
            translation?.DefaultMetaTitle ?? string.Empty, translation?.DefaultMetaDescription ?? string.Empty,
            translation?.FooterText ?? string.Empty,
            settings.GlobalSearchEnabled, settings.NewsletterEnabled, settings.PublicJobListingsEnabled, settings.DonationPageEnabled,
            settings.MaintenanceModeEnabled, translation?.MaintenanceMessage ?? string.Empty,
            settings.TurnstileSiteKey);
    }

    private async Task<string?> ResolveMediaUrlAsync(Guid? mediaAssetId, CancellationToken cancellationToken)
    {
        if (mediaAssetId is null)
        {
            return null;
        }

        var mediaAsset = await mediaAssetRepository.GetByIdAsync(mediaAssetId.Value, cancellationToken);
        return mediaAsset is null ? null : await fileStorageService.GetUrlAsync(mediaAsset.Original.FileKey, cancellationToken);
    }
}
