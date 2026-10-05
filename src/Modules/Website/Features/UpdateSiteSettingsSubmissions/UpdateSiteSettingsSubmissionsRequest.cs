namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsSubmissions;

public sealed record UpdateSiteSettingsSubmissionsRequest(byte[] RowVersion, string? SubmissionReferencePrefix);
