namespace GenclikMerkezi.Modules.Candidate.Features.SendBulkCandidateNotification;

public sealed record SendBulkCandidateNotificationRequest(IReadOnlyList<Guid>? CandidateCvIds, string Subject, string Message);
