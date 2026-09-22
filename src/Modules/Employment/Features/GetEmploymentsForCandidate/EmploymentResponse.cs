namespace GenclikMerkezi.Modules.Employment.Features.GetEmploymentsForCandidate;

// Enum'lar string olarak döner (Interview.Features.GetMyInterviewsAsCandidate.InterviewResponse ile
// aynı gerekçe - System.Text.Json bu projede JsonStringEnumConverter yapılandırılmadan sayı döner).
public sealed record EmploymentResponse(
    Guid Id,
    Guid CandidateCvId,
    Guid CompanyId,
    Guid PositionId,
    Guid? InterviewId,
    DateTime StartDateUtc,
    string Status,
    DateTime? EndDateUtc,
    string? DepartureReason,
    Guid CreatedByAdvisorId,
    DateTime CreatedAtUtc)
{
    public static EmploymentResponse FromDomain(Domain.Employment employment) => new(
        employment.Id,
        employment.CandidateCvId,
        employment.CompanyId,
        employment.PositionId,
        employment.InterviewId,
        employment.StartDateUtc,
        employment.Status.ToString(),
        employment.EndDateUtc,
        employment.DepartureReason,
        employment.CreatedByAdvisorId,
        employment.CreatedAtUtc);
}
