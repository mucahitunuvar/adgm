using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsCookieConsent;

// ADR-024 §13 (Faz 3 Görev 7): a cookiePolicyKey must reference an existing LegalDocument of Kind
// CookiePolicy - the same "resolve, then check Kind" shape UpdateSiteSettingsNewsletterCommandHandler
// uses for its own NewsletterPrivacyNoticeKey (PrivacyNotice there, CookiePolicy here), kept inline
// rather than generalizing FormDefinitionLegalReferenceGuard since this is still the only caller.
public sealed class UpdateSiteSettingsCookieConsentCommandHandler(
    ISiteSettingsRepository siteSettingsRepository,
    ILegalDocumentRepository legalDocumentRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSiteSettingsCookieConsentCommand, Result>
{
    public async Task<Result> Handle(UpdateSiteSettingsCookieConsentCommand request, CancellationToken cancellationToken)
    {
        var settings = await siteSettingsRepository.GetAsync(cancellationToken);
        var isNew = settings is null;
        settings ??= SiteSettings.CreateDefault();

        if (!isNew && !request.RowVersion.SequenceEqual(settings.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "SiteSettings.ConcurrencyConflict", "Site settings were changed by someone else. Reload and try again."));
        }

        LegalDocumentKey? cookiePolicyKey = null;
        if (!string.IsNullOrWhiteSpace(request.CookiePolicyKey))
        {
            var keyResult = LegalDocumentKey.Create(request.CookiePolicyKey);
            if (keyResult.IsFailure)
            {
                return keyResult;
            }

            var document = await legalDocumentRepository.GetByKeyAsync(keyResult.Value, cancellationToken);
            if (document is null)
            {
                return Result.Failure(Error.NotFound(
                    "LegalDocument.NotFound", $"Legal document '{keyResult.Value}' could not be found."));
            }

            if (document.Kind != LegalDocumentKind.CookiePolicy)
            {
                return Result.Failure(Error.Validation(
                    "SiteSettings.CookiePolicyKindInvalid", "The cookie policy document must have kind 'CookiePolicy'."));
            }

            cookiePolicyKey = keyResult.Value;
        }

        var translations = new List<(LanguageCode LanguageCode, UpdateSiteSettingsCookieConsentTranslationInput Input)>();
        foreach (var input in request.Translations)
        {
            var languageCodeResult = LanguageCode.Create(input.LanguageCode);
            if (languageCodeResult.IsFailure)
            {
                return languageCodeResult;
            }

            translations.Add((languageCodeResult.Value, input));
        }

        if (isNew)
        {
            siteSettingsRepository.Add(settings);
        }

        var userId = currentUserContext.UserId!.Value;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        settings.UpdateCookiePolicyKey(cookiePolicyKey, userId, now);

        foreach (var (languageCode, input) in translations)
        {
            settings.SetCookieConsentTranslation(
                languageCode, input.CookieBannerTitle, input.CookieBannerText, input.CookieCategoryNecessaryDescription,
                input.CookieCategoryAnalyticsDescription, input.CookieCategoryMarketingDescription, userId, now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidatePublicSite(cacheService);

        return Result.Success();
    }
}
