using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.CreatePersonnelNeed;

public sealed record CreatePersonnelNeedCommand(
    Guid EmploymentTypeId,
    Guid WorkLocationTypeId,
    Guid PositionId,
    Guid DepartmentId,
    int Quantity,
    Guid ProvinceId,
    Guid ExperienceLevelId,
    string? DetailsText,
    IReadOnlyList<Guid> GenderPreferenceIds,
    IReadOnlyList<Guid> MilitaryStatusPreferenceIds,
    IReadOnlyList<Guid> EducationLevelPreferenceIds,
    IReadOnlyList<Guid> DrivingLicensePreferenceIds) : IRequest<Result<CreatePersonnelNeedResponse>>;
