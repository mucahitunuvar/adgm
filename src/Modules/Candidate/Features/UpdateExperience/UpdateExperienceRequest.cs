namespace GenclikMerkezi.Modules.Candidate.Features.UpdateExperience;

public sealed record UpdateExperienceRequest(
    string CompanyName,
    Guid? PositionId,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsCurrentJob,
    Guid? SectorId,
    Guid? WorkFieldId,
    Guid? EmploymentTypeId,
    Guid? CountryId,
    Guid? ProvinceId,
    string? JobDescription);
