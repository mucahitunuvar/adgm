namespace GenclikMerkezi.Modules.Website.Features.GetFormSubmissionById;

public sealed record FormSubmissionStatusHistoryEntryResponse(string FromStatus, string ToStatus, Guid ChangedByUserId, DateTime ChangedAtUtc);
