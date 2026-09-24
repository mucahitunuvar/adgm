using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §13: per-language site name, default SEO fields and footer text. Same upsert-per-language
// shape as MediaAssetTranslation (SiteSettings.SetTranslation).
public sealed class SiteSettingsTranslation : Entity
{
    public const int MaxSiteNameLength = 150;
    public const int MaxSeoTitleLength = 200;
    public const int MaxSeoDescriptionLength = 500;
    public const int MaxFooterTextLength = 2000;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string SiteName { get; private set; } = string.Empty;

    public string DefaultSeoTitle { get; private set; } = string.Empty;

    public string DefaultSeoDescription { get; private set; } = string.Empty;

    public string FooterText { get; private set; } = string.Empty;

    private SiteSettingsTranslation(
        Guid id, LanguageCode languageCode, string siteName, string defaultSeoTitle, string defaultSeoDescription, string footerText)
        : base(id)
    {
        LanguageCode = languageCode;
        SiteName = siteName;
        DefaultSeoTitle = defaultSeoTitle;
        DefaultSeoDescription = defaultSeoDescription;
        FooterText = footerText;
    }

    private SiteSettingsTranslation()
    {
    }

    public static SiteSettingsTranslation Create(
        LanguageCode languageCode, string? siteName, string? defaultSeoTitle, string? defaultSeoDescription, string? footerText) =>
        new(
            Guid.NewGuid(), languageCode, (siteName ?? string.Empty).Trim(),
            (defaultSeoTitle ?? string.Empty).Trim(), (defaultSeoDescription ?? string.Empty).Trim(), (footerText ?? string.Empty).Trim());

    internal void Update(string? siteName, string? defaultSeoTitle, string? defaultSeoDescription, string? footerText)
    {
        SiteName = (siteName ?? string.Empty).Trim();
        DefaultSeoTitle = (defaultSeoTitle ?? string.Empty).Trim();
        DefaultSeoDescription = (defaultSeoDescription ?? string.Empty).Trim();
        FooterText = (footerText ?? string.Empty).Trim();
    }
}
