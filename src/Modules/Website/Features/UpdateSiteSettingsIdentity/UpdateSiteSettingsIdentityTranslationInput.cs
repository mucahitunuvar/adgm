namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsIdentity;

public sealed record UpdateSiteSettingsIdentityTranslationInput(
    string LanguageCode,
    string? SiteName,
    string? Tagline,
    string? DefaultMetaTitle,
    string? DefaultMetaDescription,
    string? FooterText);
