using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.Interview.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Interview.Features.GetMyInterviewsAsCandidate;

// Ownership: CandidateCvId request'ten değil, ICandidateModuleContract.GetCandidateCvByUserIdAsync
// (currentUserContext.UserId) zincirinden çözülür (GetMyCompanyQueryHandler deseni).
public sealed class GetMyInterviewsAsCandidateQueryHandler(
    IInterviewRepository interviewRepository, ICandidateModuleContract candidateModuleContract, ICurrentUserContext currentUserContext)
    : IRequestHandler<GetMyInterviewsAsCandidateQuery, Result<IReadOnlyList<InterviewResponse>>>
{
    public async Task<Result<IReadOnlyList<InterviewResponse>>> Handle(
        GetMyInterviewsAsCandidateQuery request, CancellationToken cancellationToken)
    {
        var candidate = await candidateModuleContract.GetCandidateCvByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        if (candidate is null)
        {
            return Result.Failure<IReadOnlyList<InterviewResponse>>(
                Error.NotFound("CandidateCv.NotFound", "No candidate CV was found for the current user."));
        }

        var interviews = await interviewRepository.GetByCandidateCvIdAsync(candidate.Id, cancellationToken);

        IReadOnlyList<InterviewResponse> response = interviews.Select(InterviewResponse.FromDomain).ToList();

        return Result.Success(response);
    }
}
