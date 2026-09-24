namespace GenclikMerkezi.Modules.Website.Features.GetPublicSiteSettings;

public sealed record PublicSiteSettingsTranslationResponse(
    string LanguageCode, string SiteName, string DefaultSeoTitle, string DefaultSeoDescription, string FooterText);
