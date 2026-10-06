namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsCookieConsent;

public sealed record UpdateSiteSettingsCookieConsentTranslationInput(
    string LanguageCode,
    string? CookieBannerTitle,
    string? CookieBannerText,
    string? CookieCategoryNecessaryDescription,
    string? CookieCategoryAnalyticsDescription,
    string? CookieCategoryMarketingDescription);
