namespace GenclikMerkezi.Modules.Candidate.Features.GetCandidateCvContent;

public sealed record EducationResponse(
    Guid Id,
    Guid EducationLevelId,
    DateOnly StartDate,
    string CompletionStatus,
    DateOnly? EndDate,
    Guid? DiplomaGradingSystemId,
    decimal? DiplomaGrade,
    Guid? SchoolId,
    string? SchoolNameFreeText,
    Guid? ProvinceId,
    string? Description);
