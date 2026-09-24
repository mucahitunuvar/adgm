namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettings;

public sealed record UpdateSiteSettingsTranslationInput(
    string LanguageCode,
    string? SiteName,
    string? DefaultSeoTitle,
    string? DefaultSeoDescription,
    string? FooterText);
