using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.Matching.Application.Abstractions;
using GenclikMerkezi.Modules.Matching.Features.GetSuggestionsForPersonnelNeed;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Matching.Features.GetMySuggestions;

public sealed class GetMySuggestionsQueryHandler(
    IMatchingCandidateSuggestionRepository candidateSuggestionRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICandidateModuleContract candidateModuleContract,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<GetMySuggestionsQuery, Result<IReadOnlyList<CandidateSuggestionResponse>>>
{
    public async Task<Result<IReadOnlyList<CandidateSuggestionResponse>>> Handle(
        GetMySuggestionsQuery request, CancellationToken cancellationToken)
    {
        var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        if (callerAdvisorId is null)
        {
            return Result.Failure<IReadOnlyList<CandidateSuggestionResponse>>(
                Error.Forbidden("CandidateSuggestion.NotACareerAdvisor", "Only an active career advisor may view their own suggestions."));
        }

        var suggestions = await candidateSuggestionRepository.GetBySuggestingAdvisorIdAsync(callerAdvisorId.Value, cancellationToken);

        var items = new List<CandidateSuggestionResponse>(suggestions.Count);

        foreach (var suggestion in suggestions)
        {
            var candidate = await candidateModuleContract.GetCandidateCvByIdAsync(suggestion.CandidateCvId, cancellationToken);
            var candidateName = candidate is null ? null : $"{candidate.FirstName} {candidate.LastName}";

            items.Add(CandidateSuggestionResponse.FromDomain(suggestion, candidateName));
        }

        return Result.Success<IReadOnlyList<CandidateSuggestionResponse>>(items);
    }
}
