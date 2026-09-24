namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettings;

public sealed record UpdateSiteSettingsRequest(
    UpdateSiteSettingsThemeInput Theme,
    UpdateSiteSettingsContactInput Contact,
    IReadOnlyList<UpdateSiteSettingsSocialLinkInput> SocialLinks,
    IReadOnlyList<UpdateSiteSettingsBankAccountInput> BankAccounts,
    IReadOnlyList<UpdateSiteSettingsTranslationInput> Translations,
    UpdateSiteSettingsFeatureFlagsInput FeatureFlags,
    bool MaintenanceModeEnabled,
    string? MaintenanceMessage);
