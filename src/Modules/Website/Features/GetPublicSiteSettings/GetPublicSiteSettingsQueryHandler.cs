using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicSiteSettings;

// ADR-024 §13/§15: unauthenticated - the frontend calls this once at bootstrap to render the site
// chrome (theme, contact, footer per language) before any page-specific content is fetched.
public sealed class GetPublicSiteSettingsQueryHandler(
    ISiteSettingsRepository siteSettingsRepository, IMediaAssetRepository mediaAssetRepository, IFileStorageService fileStorageService)
    : IRequestHandler<GetPublicSiteSettingsQuery, Result<PublicSiteSettingsResponse>>
{
    public async Task<Result<PublicSiteSettingsResponse>> Handle(GetPublicSiteSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await siteSettingsRepository.GetAsync(cancellationToken) ?? SiteSettings.CreateDefault();

        var themeResponse = new PublicSiteThemeResponse(
            await ResolveMediaUrlAsync(settings.Theme.LogoLightMediaAssetId, cancellationToken),
            await ResolveMediaUrlAsync(settings.Theme.LogoDarkMediaAssetId, cancellationToken),
            await ResolveMediaUrlAsync(settings.Theme.FaviconMediaAssetId, cancellationToken),
            settings.Theme.PrimaryColorHex, settings.Theme.SecondaryColorHex, settings.Theme.FontFamily);

        var contactResponse = new PublicContactInfoResponse(
            settings.Contact.Address, settings.Contact.Phone, settings.Contact.Email, settings.Contact.WhatsApp, settings.Contact.MapEmbedUrl);

        var socialLinks = settings.SocialLinks
            .OrderBy(l => l.SortOrder)
            .Select(l => new PublicSocialLinkResponse(l.Platform, l.Url, l.SortOrder))
            .ToList();

        // DonationPageEnabled is false in this project, but the accounts are still returned - the
        // frontend simply never renders the page/menu item that would show them (ADR-024 §13).
        var bankAccounts = settings.BankAccounts
            .Where(a => a.IsActive)
            .OrderBy(a => a.SortOrder)
            .Select(a => new PublicBankAccountResponse(a.Iban.Value, a.BankName, a.AccountHolder, a.Description, a.SortOrder))
            .ToList();

        var translations = settings.Translations
            .Select(t => new PublicSiteSettingsTranslationResponse(t.LanguageCode.Value, t.SiteName, t.DefaultSeoTitle, t.DefaultSeoDescription, t.FooterText))
            .ToList();

        return Result.Success(new PublicSiteSettingsResponse(
            themeResponse, contactResponse, socialLinks, bankAccounts, translations,
            settings.GlobalSearchEnabled, settings.NewsletterEnabled, settings.PublicJobListingsEnabled,
            settings.DonationPageEnabled, settings.MaintenanceModeEnabled, settings.MaintenanceMessage));
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
