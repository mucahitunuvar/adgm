namespace GenclikMerkezi.Modules.Website.Features.GetSiteSettings;

public sealed record SiteSettingsTranslationResponse(
    string LanguageCode,
    string SiteName,
    string Tagline,
    string DefaultMetaTitle,
    string DefaultMetaDescription,
    string FooterText,
    string MaintenanceMessage,
    string CookieBannerTitle,
    string CookieBannerText,
    string CookieCategoryNecessaryDescription,
    string CookieCategoryAnalyticsDescription,
    string CookieCategoryMarketingDescription);
