using GenclikMerkezi.Modules.Employer.Features.GetPublishedJobs;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.GetMyCompanyJobs;

public sealed record GetMyCompanyJobsQuery : IRequest<Result<IReadOnlyList<JobResponse>>>;
