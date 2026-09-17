namespace GenclikMerkezi.Modules.Candidate.Features.AddEducation;

public sealed record AddEducationRequest(
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
