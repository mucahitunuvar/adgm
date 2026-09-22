using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Modules.Matching.Application.Abstractions;
using GenclikMerkezi.Modules.Matching.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Matching.Features.AcceptCandidateSuggestion;

// ApproveJobCommandHandler'ın iki contract çağrısı zincirlenmiş hali (master prompt): çağıranın
// PersonnelNeed'in ait olduğu firmanın atanmış danışmanı olduğu IPersonnelNeedModuleContract ->
// ICompanyModuleContract -> ICareerAdvisorModuleContract zinciriyle doğrulanır. Kabul, aynı
// PersonnelNeed'e yapılmış diğer tüm Onerildi önerileri de (pozisyon dolduğu için) aynı transaction'da
// reddeder; IPersonnelNeedModuleContract.CloseAsync commit'ten SONRA çağrılır (ADR-022 §3/§6 deseni) -
// başarısız olsa bile komut Success döner, Accept zaten commit edildi (bkz. plan).
public sealed class AcceptCandidateSuggestionCommandHandler(
    IMatchingCandidateSuggestionRepository candidateSuggestionRepository,
    IPersonnelNeedModuleContract personnelNeedModuleContract,
    ICompanyModuleContract companyModuleContract,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(MatchingModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<AcceptCandidateSuggestionCommand, Result>
{
    public async Task<Result> Handle(AcceptCandidateSuggestionCommand request, CancellationToken cancellationToken)
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

        var acceptResult = suggestion.Accept(callerAdvisorId.Value, DateTime.UtcNow);

        if (acceptResult.IsFailure)
        {
            return acceptResult;
        }

        var siblingSuggestions = await candidateSuggestionRepository.GetByPersonnelNeedIdAsync(
            suggestion.PersonnelNeedId, cancellationToken);

        foreach (var sibling in siblingSuggestions)
        {
            if (sibling.Id != suggestion.Id && sibling.Status == CandidateSuggestionStatus.Onerildi)
            {
                sibling.Reject(callerAdvisorId.Value, DateTime.UtcNow);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await personnelNeedModuleContract.CloseAsync(
            suggestion.PersonnelNeedId, callerAdvisorId.Value, suggestion.CandidateCvId, cancellationToken);

        return Result.Success();
    }
}
