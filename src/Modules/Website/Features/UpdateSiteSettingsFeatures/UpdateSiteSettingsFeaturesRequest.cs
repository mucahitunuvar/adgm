namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsFeatures;

public sealed record UpdateSiteSettingsFeaturesRequest(
    byte[] RowVersion,
    bool GlobalSearchEnabled,
    bool NewsletterEnabled,
    bool PublicJobListingsEnabled,
    bool DonationPageEnabled);
