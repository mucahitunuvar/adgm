using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §13: per-language site name, default SEO fields, footer text and maintenance message.
// Same upsert-per-language shape as MediaAssetTranslation (SiteSettings.SetTranslation).
// MaintenanceMessage lives here (not on SiteSettings itself) because the maintenance banner must
// speak the visitor's language - only MaintenanceModeEnabled (the on/off switch) is global.
public sealed class SiteSettingsTranslation : Entity
{
    public const int MaxSiteNameLength = 150;
    public const int MaxSeoTitleLength = 200;
    public const int MaxSeoDescriptionLength = 500;
    public const int MaxFooterTextLength = 2000;
    public const int MaxMaintenanceMessageLength = 500;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string SiteName { get; private set; } = string.Empty;

    public string DefaultSeoTitle { get; private set; } = string.Empty;

    public string DefaultSeoDescription { get; private set; } = string.Empty;

    public string FooterText { get; private set; } = string.Empty;

    public string MaintenanceMessage { get; private set; } = string.Empty;

    private SiteSettingsTranslation(
        Guid id,
        LanguageCode languageCode,
        string siteName,
        string defaultSeoTitle,
        string defaultSeoDescription,
        string footerText,
        string maintenanceMessage)
        : base(id)
    {
        LanguageCode = languageCode;
        SiteName = siteName;
        DefaultSeoTitle = defaultSeoTitle;
        DefaultSeoDescription = defaultSeoDescription;
        FooterText = footerText;
        MaintenanceMessage = maintenanceMessage;
    }

    private SiteSettingsTranslation()
    {
    }

    public static SiteSettingsTranslation Create(
        LanguageCode languageCode,
        string? siteName,
        string? defaultSeoTitle,
        string? defaultSeoDescription,
        string? footerText,
        string? maintenanceMessage) =>
        new(
            Guid.NewGuid(), languageCode, (siteName ?? string.Empty).Trim(),
            (defaultSeoTitle ?? string.Empty).Trim(), (defaultSeoDescription ?? string.Empty).Trim(),
            (footerText ?? string.Empty).Trim(), (maintenanceMessage ?? string.Empty).Trim());

    internal void Update(
        string? siteName, string? defaultSeoTitle, string? defaultSeoDescription, string? footerText, string? maintenanceMessage)
    {
        SiteName = (siteName ?? string.Empty).Trim();
        DefaultSeoTitle = (defaultSeoTitle ?? string.Empty).Trim();
        DefaultSeoDescription = (defaultSeoDescription ?? string.Empty).Trim();
        FooterText = (footerText ?? string.Empty).Trim();
        MaintenanceMessage = (maintenanceMessage ?? string.Empty).Trim();
    }
}
