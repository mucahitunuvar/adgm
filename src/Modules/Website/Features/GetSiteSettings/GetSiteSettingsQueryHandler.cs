using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetSiteSettings;

public sealed class GetSiteSettingsQueryHandler(
    ISiteSettingsRepository siteSettingsRepository, IMediaAssetRepository mediaAssetRepository, IFileStorageService fileStorageService)
    : IRequestHandler<GetSiteSettingsQuery, Result<SiteSettingsResponse>>
{
    public async Task<Result<SiteSettingsResponse>> Handle(GetSiteSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await siteSettingsRepository.GetAsync(cancellationToken) ?? SiteSettings.CreateDefault();

        var themeResponse = new SiteThemeResponse(settings.Theme.PrimaryColorHex, settings.Theme.SecondaryColorHex, settings.Theme.FontFamily);

        var contactResponse = new ContactInfoResponse(
            settings.Contact.Address, settings.Contact.Phone, settings.Contact.Email, settings.Contact.WhatsApp, settings.Contact.MapEmbedUrl);

        var socialLinks = settings.SocialLinks
            .Select(l => new SocialLinkResponse(l.Id, l.Platform, l.Url, l.SortOrder))
            .ToList();

        var bankAccounts = settings.BankAccounts
            .Select(a => new BankAccountResponse(
                a.Id, a.Iban.Value, a.BankName, a.AccountHolder, a.Currency.ToString(), a.Description, a.SortOrder, a.IsActive))
            .ToList();

        var translations = settings.Translations
            .Select(t => new SiteSettingsTranslationResponse(
                t.LanguageCode.Value, t.SiteName, t.Tagline, t.DefaultMetaTitle, t.DefaultMetaDescription, t.FooterText, t.MaintenanceMessage))
            .ToList();

        return Result.Success(new SiteSettingsResponse(
            settings.RowVersion,
            settings.LogoLightMediaAssetId, await ResolveMediaUrlAsync(settings.LogoLightMediaAssetId, cancellationToken),
            settings.LogoDarkMediaAssetId, await ResolveMediaUrlAsync(settings.LogoDarkMediaAssetId, cancellationToken),
            settings.FaviconMediaAssetId, await ResolveMediaUrlAsync(settings.FaviconMediaAssetId, cancellationToken),
            settings.DefaultOgImageMediaId, await ResolveMediaUrlAsync(settings.DefaultOgImageMediaId, cancellationToken),
            themeResponse, contactResponse, socialLinks, bankAccounts, translations,
            settings.GlobalSearchEnabled, settings.NewsletterEnabled, settings.PublicJobListingsEnabled,
            settings.DonationPageEnabled, settings.BotProtectionEnabled, settings.TurnstileSiteKey,
            settings.MaintenanceModeEnabled, settings.UpdatedAtUtc));
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
