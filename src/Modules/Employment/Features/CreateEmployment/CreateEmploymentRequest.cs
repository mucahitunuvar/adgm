namespace GenclikMerkezi.Modules.Employment.Features.CreateEmployment;

public sealed record CreateEmploymentRequest(
    Guid CandidateCvId, Guid CompanyId, Guid PositionId, Guid? InterviewId, DateTime StartDateUtc);
