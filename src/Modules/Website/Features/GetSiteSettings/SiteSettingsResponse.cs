namespace GenclikMerkezi.Modules.Website.Features.GetSiteSettings;

// RowVersion (base64 in JSON) is the optimistic-concurrency token every grouped PUT
// (UpdateSiteSettings{Identity,Theme,Contact,BankAccounts,Features,Maintenance,BotProtection})
// requires back - the admin UI must round-trip whatever value this GET last returned.
public sealed record SiteSettingsResponse(
    byte[] RowVersion,
    Guid? LogoLightMediaAssetId,
    string? LogoLightUrl,
    Guid? LogoDarkMediaAssetId,
    string? LogoDarkUrl,
    Guid? FaviconMediaAssetId,
    string? FaviconUrl,
    Guid? DefaultOgImageMediaId,
    string? DefaultOgImageUrl,
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
    string TurnstileSiteKey,
    bool MaintenanceModeEnabled,
    DateTime? UpdatedAtUtc);
