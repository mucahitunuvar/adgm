using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.Interview.Application.Abstractions;
using GenclikMerkezi.Modules.Interview.Features.GetMyInterviewsAsCandidate;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Interview.Features.GetInterviewsToOrganize;

// GetJobsPendingReviewQueryHandler'daki gerekçeyle aynı: çağıranın kendi CareerAdvisorId'si
// ICareerAdvisorModuleContract üzerinden çözülür.
public sealed class GetInterviewsToOrganizeQueryHandler(
    IInterviewRepository interviewRepository, ICareerAdvisorModuleContract careerAdvisorModuleContract, ICurrentUserContext currentUserContext)
    : IRequestHandler<GetInterviewsToOrganizeQuery, Result<IReadOnlyList<InterviewResponse>>>
{
    public async Task<Result<IReadOnlyList<InterviewResponse>>> Handle(
        GetInterviewsToOrganizeQuery request, CancellationToken cancellationToken)
    {
        var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        if (callerAdvisorId is null)
        {
            return Result.Failure<IReadOnlyList<InterviewResponse>>(
                Error.Forbidden("Interview.NotACareerAdvisor", "Only an active career advisor may view the interview queue."));
        }

        var interviews = await interviewRepository.GetPendingByOrganizingAdvisorIdAsync(callerAdvisorId.Value, cancellationToken);

        IReadOnlyList<InterviewResponse> response = interviews.Select(InterviewResponse.FromDomain).ToList();

        return Result.Success(response);
    }
}
