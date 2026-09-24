using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §13 / Görev 6: per-language site identity text (site name, tagline, default SEO meta,
// footer) and maintenance message. Split into two independent partial-update methods
// (UpdateIdentityFields / UpdateMaintenanceMessage) because SiteSettings exposes them through two
// separate grouped endpoints ("identity" and "maintenance") - updating one group must never
// overwrite fields that belong to the other.
public sealed class SiteSettingsTranslation : Entity
{
    public const int MaxSiteNameLength = 150;
    public const int MaxTaglineLength = 200;
    public const int MaxMetaTitleLength = 200;
    public const int MaxMetaDescriptionLength = 500;
    public const int MaxFooterTextLength = 2000;
    public const int MaxMaintenanceMessageLength = 500;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string SiteName { get; private set; } = string.Empty;

    public string Tagline { get; private set; } = string.Empty;

    public string DefaultMetaTitle { get; private set; } = string.Empty;

    public string DefaultMetaDescription { get; private set; } = string.Empty;

    public string FooterText { get; private set; } = string.Empty;

    public string MaintenanceMessage { get; private set; } = string.Empty;

    private SiteSettingsTranslation(
        Guid id,
        LanguageCode languageCode,
        string siteName,
        string tagline,
        string defaultMetaTitle,
        string defaultMetaDescription,
        string footerText,
        string maintenanceMessage)
        : base(id)
    {
        LanguageCode = languageCode;
        SiteName = siteName;
        Tagline = tagline;
        DefaultMetaTitle = defaultMetaTitle;
        DefaultMetaDescription = defaultMetaDescription;
        FooterText = footerText;
        MaintenanceMessage = maintenanceMessage;
    }

    private SiteSettingsTranslation()
    {
    }

    public static SiteSettingsTranslation Create(
        LanguageCode languageCode,
        string? siteName,
        string? tagline,
        string? defaultMetaTitle,
        string? defaultMetaDescription,
        string? footerText,
        string? maintenanceMessage) =>
        new(
            Guid.NewGuid(), languageCode, (siteName ?? string.Empty).Trim(), (tagline ?? string.Empty).Trim(),
            (defaultMetaTitle ?? string.Empty).Trim(), (defaultMetaDescription ?? string.Empty).Trim(),
            (footerText ?? string.Empty).Trim(), (maintenanceMessage ?? string.Empty).Trim());

    internal void UpdateIdentityFields(string? siteName, string? tagline, string? defaultMetaTitle, string? defaultMetaDescription, string? footerText)
    {
        SiteName = (siteName ?? string.Empty).Trim();
        Tagline = (tagline ?? string.Empty).Trim();
        DefaultMetaTitle = (defaultMetaTitle ?? string.Empty).Trim();
        DefaultMetaDescription = (defaultMetaDescription ?? string.Empty).Trim();
        FooterText = (footerText ?? string.Empty).Trim();
    }

    internal void UpdateMaintenanceMessage(string? maintenanceMessage)
    {
        MaintenanceMessage = (maintenanceMessage ?? string.Empty).Trim();
    }
}
