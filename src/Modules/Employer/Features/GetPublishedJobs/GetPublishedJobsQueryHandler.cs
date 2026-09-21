using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.GetPublishedJobs;

public sealed class GetPublishedJobsQueryHandler(IJobRepository jobRepository)
    : IRequestHandler<GetPublishedJobsQuery, Result<IReadOnlyList<JobResponse>>>
{
    public async Task<Result<IReadOnlyList<JobResponse>>> Handle(GetPublishedJobsQuery request, CancellationToken cancellationToken)
    {
        var jobs = await jobRepository.GetPublishedAsync(cancellationToken);

        IReadOnlyList<JobResponse> response = jobs.Select(JobResponse.FromDomain).ToList();

        return Result.Success(response);
    }
}
