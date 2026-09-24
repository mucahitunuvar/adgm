using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettings;

public sealed class UpdateSiteSettingsCommandHandler(
    ISiteSettingsRepository siteSettingsRepository,
    IMediaAssetRepository mediaAssetRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSiteSettingsCommand, Result>
{
    public async Task<Result> Handle(UpdateSiteSettingsCommand request, CancellationToken cancellationToken)
    {
        foreach (var mediaAssetId in new[]
                 {
                     request.Theme.LogoLightMediaAssetId, request.Theme.LogoDarkMediaAssetId, request.Theme.FaviconMediaAssetId,
                 })
        {
            if (mediaAssetId is null)
            {
                continue;
            }

            var mediaAsset = await mediaAssetRepository.GetByIdAsync(mediaAssetId.Value, cancellationToken);
            if (mediaAsset is null)
            {
                return Result.Failure(Error.NotFound(
                    "SiteSettings.MediaAssetNotFound", $"Media asset '{mediaAssetId}' could not be found."));
            }
        }

        var themeResult = SiteTheme.Create(
            request.Theme.LogoLightMediaAssetId, request.Theme.LogoDarkMediaAssetId, request.Theme.FaviconMediaAssetId,
            request.Theme.PrimaryColorHex, request.Theme.SecondaryColorHex, request.Theme.FontFamily);
        if (themeResult.IsFailure)
        {
            return themeResult;
        }

        var contactResult = ContactInfo.Create(
            request.Contact.Address, request.Contact.Phone, request.Contact.Email, request.Contact.WhatsApp, request.Contact.MapEmbedUrl);
        if (contactResult.IsFailure)
        {
            return contactResult;
        }

        var socialLinks = request.SocialLinks
            .Select(l => SocialLink.Create(l.Platform, l.Url, l.SortOrder))
            .ToList();

        var bankAccounts = new List<BankAccount>();
        foreach (var input in request.BankAccounts)
        {
            var ibanResult = Iban.Create(input.Iban);
            if (ibanResult.IsFailure)
            {
                return ibanResult;
            }

            bankAccounts.Add(BankAccount.Create(
                ibanResult.Value, input.BankName, input.AccountHolder, input.Description, input.SortOrder, input.IsActive));
        }

        var translations = new List<(LanguageCode LanguageCode, string? SiteName, string? SeoTitle, string? SeoDescription, string? FooterText)>();
        foreach (var input in request.Translations)
        {
            var languageCodeResult = LanguageCode.Create(input.LanguageCode);
            if (languageCodeResult.IsFailure)
            {
                return languageCodeResult;
            }

            translations.Add((languageCodeResult.Value, input.SiteName, input.DefaultSeoTitle, input.DefaultSeoDescription, input.FooterText));
        }

        var settings = await siteSettingsRepository.GetAsync(cancellationToken);
        var isNew = settings is null;
        settings ??= SiteSettings.CreateDefault();
        if (isNew)
        {
            siteSettingsRepository.Add(settings);
        }

        var userId = currentUserContext.UserId!.Value;
        var now = DateTime.UtcNow;

        settings.UpdateTheme(themeResult.Value, userId, now);
        settings.UpdateContact(contactResult.Value, userId, now);
        settings.ReplaceSocialLinks(socialLinks, userId, now);
        settings.ReplaceBankAccounts(bankAccounts, userId, now);

        foreach (var (languageCode, siteName, seoTitle, seoDescription, footerText) in translations)
        {
            settings.SetTranslation(languageCode, siteName, seoTitle, seoDescription, footerText, userId, now);
        }

        settings.UpdateFeatureFlags(
            request.FeatureFlags.GlobalSearchEnabled, request.FeatureFlags.NewsletterEnabled,
            request.FeatureFlags.PublicJobListingsEnabled, request.FeatureFlags.DonationPageEnabled,
            request.FeatureFlags.BotProtectionEnabled, userId, now);

        settings.SetMaintenanceMode(request.MaintenanceModeEnabled, request.MaintenanceMessage, userId, now);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
