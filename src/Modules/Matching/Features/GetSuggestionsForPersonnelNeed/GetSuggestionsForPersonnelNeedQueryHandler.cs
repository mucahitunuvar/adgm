using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Modules.Matching.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Matching.Features.GetSuggestionsForPersonnelNeed;

// AcceptCandidateSuggestionCommandHandler'daki reviewer-zinciriyle aynı yetki kontrolü - yalnızca
// ihtiyaç sahibi firmanın atanmış danışmanı önerileri görebilir.
public sealed class GetSuggestionsForPersonnelNeedQueryHandler(
    IMatchingCandidateSuggestionRepository candidateSuggestionRepository,
    IPersonnelNeedModuleContract personnelNeedModuleContract,
    ICompanyModuleContract companyModuleContract,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICandidateModuleContract candidateModuleContract,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<GetSuggestionsForPersonnelNeedQuery, Result<IReadOnlyList<CandidateSuggestionResponse>>>
{
    public async Task<Result<IReadOnlyList<CandidateSuggestionResponse>>> Handle(
        GetSuggestionsForPersonnelNeedQuery request, CancellationToken cancellationToken)
    {
        var personnelNeed = await personnelNeedModuleContract.GetByIdAsync(request.PersonnelNeedId, cancellationToken);

        if (personnelNeed is null)
        {
            return Result.Failure<IReadOnlyList<CandidateSuggestionResponse>>(
                Error.NotFound("PersonnelNeed.NotFound", "The specified personnel need could not be found."));
        }

        var companyCareerAdvisorId = await companyModuleContract.GetCareerAdvisorIdForCompanyAsync(
            personnelNeed.CompanyId, cancellationToken);

        var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        if (callerAdvisorId is null || companyCareerAdvisorId != callerAdvisorId)
        {
            return Result.Failure<IReadOnlyList<CandidateSuggestionResponse>>(Error.Forbidden(
                "CandidateSuggestion.NotAssignedAdvisor",
                "Only the personnel need's company's assigned career advisor may view its suggestions."));
        }

        var suggestions = await candidateSuggestionRepository.GetByPersonnelNeedIdAsync(request.PersonnelNeedId, cancellationToken);

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
