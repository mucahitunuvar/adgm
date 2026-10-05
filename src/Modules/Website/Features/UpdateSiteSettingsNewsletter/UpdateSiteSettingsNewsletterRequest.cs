namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsNewsletter;

public sealed record UpdateSiteSettingsNewsletterRequest(byte[] RowVersion, string? NewsletterPrivacyNoticeKey);
