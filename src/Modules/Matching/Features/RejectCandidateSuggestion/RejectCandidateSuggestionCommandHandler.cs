using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Modules.Matching.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Matching.Features.RejectCandidateSuggestion;

// AcceptCandidateSuggestionCommandHandler'daki gerekçeyle aynı reviewer-zinciri (ApproveJobCommandHandler
// deseni). Yalnızca suggestion.Reject() - PersonnelNeed'e dokunmaz, hâlâ GenelHavuzda kalır, başka
// danışman önerebilir.
public sealed class RejectCandidateSuggestionCommandHandler(
    IMatchingCandidateSuggestionRepository candidateSuggestionRepository,
    IPersonnelNeedModuleContract personnelNeedModuleContract,
    ICompanyModuleContract companyModuleContract,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(MatchingModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<RejectCandidateSuggestionCommand, Result>
{
    public async Task<Result> Handle(RejectCandidateSuggestionCommand request, CancellationToken cancellationToken)
    {
        var suggestion = await candidateSuggestionRepository.GetByIdAsync(request.CandidateSuggestionId, cancellationToken);

        if (suggestion is null)
        {
            return Result.Failure(Error.NotFound("CandidateSuggestion.NotFound", "The specified candidate suggestion could not be found."));
        }

        var personnelNeed = await personnelNeedModuleContract.GetByIdAsync(suggestion.PersonnelNeedId, cancellationToken);

        if (personnelNeed is null)
        {
            return Result.Failure(Error.NotFound("PersonnelNeed.NotFound", "The specified personnel need could not be found."));
        }

        var companyCareerAdvisorId = await companyModuleContract.GetCareerAdvisorIdForCompanyAsync(
            personnelNeed.CompanyId, cancellationToken);

        var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        if (callerAdvisorId is null || companyCareerAdvisorId != callerAdvisorId)
        {
            return Result.Failure(Error.Forbidden(
                "CandidateSuggestion.NotAssignedAdvisor",
                "Only the personnel need's company's assigned career advisor may decide on this suggestion."));
        }

        var rejectResult = suggestion.Reject(callerAdvisorId.Value, DateTime.UtcNow);

        if (rejectResult.IsFailure)
        {
            return rejectResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
