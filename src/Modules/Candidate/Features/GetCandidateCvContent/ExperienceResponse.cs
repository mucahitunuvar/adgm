namespace GenclikMerkezi.Modules.Candidate.Features.GetCandidateCvContent;

public sealed record ExperienceResponse(
    Guid Id,
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
