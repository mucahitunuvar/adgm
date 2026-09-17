namespace GenclikMerkezi.Modules.Candidate.Features.UpdateEducation;

public sealed record UpdateEducationRequest(
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
