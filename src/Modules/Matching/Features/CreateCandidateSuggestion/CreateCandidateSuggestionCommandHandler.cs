using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Modules.Matching.Application.Abstractions;
using GenclikMerkezi.Modules.Matching.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Matching.Features.CreateCandidateSuggestion;

public sealed class CreateCandidateSuggestionCommandHandler(
    IMatchingCandidateSuggestionRepository candidateSuggestionRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICandidateModuleContract candidateModuleContract,
    IPersonnelNeedModuleContract personnelNeedModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(MatchingModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCandidateSuggestionCommand, Result<CreateCandidateSuggestionResponse>>
{
    public async Task<Result<CreateCandidateSuggestionResponse>> Handle(
        CreateCandidateSuggestionCommand request, CancellationToken cancellationToken)
    {
        var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        if (callerAdvisorId is null)
        {
            return Result.Failure<CreateCandidateSuggestionResponse>(
                Error.Forbidden("CandidateSuggestion.NotACareerAdvisor", "Only an active career advisor may suggest a candidate."));
        }

        var candidateAdvisorId = await candidateModuleContract.GetCareerAdvisorIdForCandidateAsync(
            request.CandidateCvId, cancellationToken);

        if (candidateAdvisorId != callerAdvisorId)
        {
            return Result.Failure<CreateCandidateSuggestionResponse>(
                Error.Forbidden("CandidateSuggestion.NotOwnCandidate", "You may only suggest your own candidates."));
        }

        var isInGeneralPool = await personnelNeedModuleContract.IsInGeneralPoolAsync(request.PersonnelNeedId, cancellationToken);

        if (!isInGeneralPool)
        {
            return Result.Failure<CreateCandidateSuggestionResponse>(Error.Conflict(
                "CandidateSuggestion.NotInGeneralPool", "The specified personnel need is not currently in the general pool."));
        }

        var alreadySuggested = await candidateSuggestionRepository.ExistsForPersonnelNeedAndCandidateAsync(
            request.PersonnelNeedId, request.CandidateCvId, cancellationToken);

        if (alreadySuggested)
        {
            return Result.Failure<CreateCandidateSuggestionResponse>(Error.Conflict(
                "CandidateSuggestion.AlreadySuggested", "This candidate has already been suggested for this personnel need."));
        }

        var suggestion = CandidateSuggestion.Create(
            request.PersonnelNeedId, request.CandidateCvId, callerAdvisorId.Value, DateTime.UtcNow);

        candidateSuggestionRepository.Add(suggestion);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateCandidateSuggestionResponse(suggestion.Id));
    }
}
