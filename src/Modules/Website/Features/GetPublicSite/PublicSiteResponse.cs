namespace GenclikMerkezi.Modules.Website.Features.GetPublicSite;

// ADR-024 §13/§15. Kept intentionally flat and additive so Faz 2 (menu, pop-up) can add fields
// without reshaping what already exists - every consumer of this contract only ever adds new
// optional properties to it, never renames or removes one.
public sealed record PublicSiteResponse(
    IReadOnlyList<PublicSiteLanguageResponse> Languages,
    string ResolvedLanguageCode,
    PublicSiteThemeResponse Theme,
    PublicContactInfoResponse Contact,
    IReadOnlyList<PublicSocialLinkResponse> SocialLinks,
    IReadOnlyList<PublicBankAccountResponse> BankAccounts,
    string SiteName,
    string DefaultSeoTitle,
    string DefaultSeoDescription,
    string FooterText,
    bool GlobalSearchEnabled,
    bool NewsletterEnabled,
    bool PublicJobListingsEnabled,
    bool DonationPageEnabled,
    bool MaintenanceModeEnabled,
    string MaintenanceMessage,
    string TurnstileSiteKey);
