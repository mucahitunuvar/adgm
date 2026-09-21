using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.GetPublishedJobs;

public sealed record GetPublishedJobsQuery : IRequest<Result<IReadOnlyList<JobResponse>>>;
