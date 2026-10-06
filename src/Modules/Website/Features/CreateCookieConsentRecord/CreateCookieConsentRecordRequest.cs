namespace GenclikMerkezi.Modules.Website.Features.CreateCookieConsentRecord;

public sealed record CreateCookieConsentRecordRequest(Guid ConsentId, IReadOnlyList<string>? Categories, int PolicyVersion, string? Action);
