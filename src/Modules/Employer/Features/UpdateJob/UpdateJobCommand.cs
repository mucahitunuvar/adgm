using GenclikMerkezi.Modules.Employer.Features.CreateJob;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.UpdateJob;

public sealed record UpdateJobCommand(
    Guid JobId,
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
    IReadOnlyList<LanguageRequirementRequest> LanguageRequirements) : IRequest<Result>;
