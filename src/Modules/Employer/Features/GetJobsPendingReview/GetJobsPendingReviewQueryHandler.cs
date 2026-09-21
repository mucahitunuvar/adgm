using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Features.GetPublishedJobs;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.GetJobsPendingReview;

// ApproveJobCommandHandler'daki gerekçeyle aynı: çağıranın kendi CareerAdvisorId'si
// ICareerAdvisorModuleContract üzerinden çözülür (Identity.User.Id, CareerAdvisor.Id ile aynı değil).
public sealed class GetJobsPendingReviewQueryHandler(
    IJobRepository jobRepository, ICareerAdvisorModuleContract careerAdvisorModuleContract, ICurrentUserContext currentUserContext)
    : IRequestHandler<GetJobsPendingReviewQuery, Result<IReadOnlyList<JobResponse>>>
{
    public async Task<Result<IReadOnlyList<JobResponse>>> Handle(
        GetJobsPendingReviewQuery request, CancellationToken cancellationToken)
    {
        var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        if (callerAdvisorId is null)
        {
            return Result.Failure<IReadOnlyList<JobResponse>>(
                Error.Forbidden("Job.NotACareerAdvisor", "Only an active career advisor may view the review queue."));
        }

        var jobs = await jobRepository.GetPendingReviewByAdvisorIdAsync(callerAdvisorId.Value, cancellationToken);

        IReadOnlyList<JobResponse> response = jobs.Select(JobResponse.FromDomain).ToList();

        return Result.Success(response);
    }
}
