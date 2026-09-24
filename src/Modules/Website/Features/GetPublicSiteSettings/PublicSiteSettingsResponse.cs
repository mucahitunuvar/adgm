namespace GenclikMerkezi.Modules.Website.Features.GetPublicSiteSettings;

public sealed record PublicSiteSettingsResponse(
    PublicSiteThemeResponse Theme,
    PublicContactInfoResponse Contact,
    IReadOnlyList<PublicSocialLinkResponse> SocialLinks,
    IReadOnlyList<PublicBankAccountResponse> BankAccounts,
    IReadOnlyList<PublicSiteSettingsTranslationResponse> Translations,
    bool GlobalSearchEnabled,
    bool NewsletterEnabled,
    bool PublicJobListingsEnabled,
    bool DonationPageEnabled,
    bool MaintenanceModeEnabled,
    string? MaintenanceMessage);
