namespace GenclikMerkezi.Modules.Employer.Features.CreateJob;

public sealed record CreateJobRequest(
    string Title,
    bool IsForDisabledCandidates,
    Guid EmploymentTypeId,
    Guid WorkLocationTypeId,
    Guid PositionId,
    Guid DepartmentId,
    Guid ProvinceId,
    string? DescriptionHtml,
    Guid ExperienceLevelId,
    IReadOnlyList<Guid> GenderPreferenceIds,
    IReadOnlyList<Guid> MilitaryStatusPreferenceIds,
    IReadOnlyList<Guid> EducationLevelPreferenceIds,
    IReadOnlyList<Guid> DrivingLicensePreferenceIds,
    IReadOnlyList<LanguageRequirementRequest> LanguageRequirements);
