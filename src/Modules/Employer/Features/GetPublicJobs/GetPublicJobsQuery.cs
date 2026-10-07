using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.GetPublicJobs;

public sealed record GetPublicJobsQuery(
    Guid? ProvinceId,
    Guid? EmploymentTypeId,
    Guid? WorkLocationTypeId,
    Guid? PositionId,
    Guid? DepartmentId,
    Guid? CompanyId,
    bool? IsForDisabledCandidates,
    string? Q) : PagedRequest, IRequest<Result<PagedResult<PublicJobListItemResponse>>>;
