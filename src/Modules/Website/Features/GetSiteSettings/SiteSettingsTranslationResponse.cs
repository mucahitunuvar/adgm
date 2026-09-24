namespace GenclikMerkezi.Modules.Website.Features.GetSiteSettings;

public sealed record SiteSettingsTranslationResponse(
    string LanguageCode,
    string SiteName,
    string DefaultSeoTitle,
    string DefaultSeoDescription,
    string FooterText,
    string MaintenanceMessage);
