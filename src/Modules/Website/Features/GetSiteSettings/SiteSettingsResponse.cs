namespace GenclikMerkezi.Modules.Website.Features.GetSiteSettings;

public sealed record SiteSettingsResponse(
    SiteThemeResponse Theme,
    ContactInfoResponse Contact,
    IReadOnlyList<SocialLinkResponse> SocialLinks,
    IReadOnlyList<BankAccountResponse> BankAccounts,
    IReadOnlyList<SiteSettingsTranslationResponse> Translations,
    bool GlobalSearchEnabled,
    bool NewsletterEnabled,
    bool PublicJobListingsEnabled,
    bool DonationPageEnabled,
    bool BotProtectionEnabled,
    bool MaintenanceModeEnabled,
    string? MaintenanceMessage,
    DateTime? UpdatedAtUtc);
