namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsEvents;

public sealed record UpdateSiteSettingsEventsRequest(byte[] RowVersion, string? EventPrivacyNoticeKey);
