namespace GenclikMerkezi.Modules.Interview.Features.GetMyInterviewsAsCandidate;

// GetMyInterviewsAsEmployer/GetInterviewsToOrganize de bu paylaşılan response'u kullanıyor
// (JobResponse/GetPublishedJobs deseni). Enum'lar string olarak döner (CandidateNoteItemResponse.
// NoteType.ToString() deseni).
public sealed record InterviewResponse(
    Guid Id,
    Guid CandidateCvId,
    Guid CompanyId,
    Guid OrganizingAdvisorId,
    string Status,
    DateTime? ScheduledAtUtc,
    string? Outcome,
    string? ResultNotes,
    string RequestedByRole,
    DateTime CreatedAtUtc)
{
    public static InterviewResponse FromDomain(Domain.Interview interview) => new(
        interview.Id,
        interview.CandidateCvId,
        interview.CompanyId,
        interview.OrganizingAdvisorId,
        interview.Status.ToString(),
        interview.ScheduledAtUtc,
        interview.Outcome?.ToString(),
        interview.ResultNotes,
        interview.RequestedByRole.ToString(),
        interview.CreatedAtUtc);
}
