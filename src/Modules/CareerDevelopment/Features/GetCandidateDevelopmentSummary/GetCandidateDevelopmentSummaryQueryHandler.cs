using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.GetCandidateDevelopmentSummary;

// Tek query, rol-bazlı ownership dalı (iki ayrı endpoint yerine) - GetEmploymentsForCandidateQuery
// Handler'daki desenin aynısı: çağıran ya adayın kendisi ya da adayın GÜNCEL danışmanı olabilir.
// İkisi de değilse Forbidden - endpoint bu yüzden spesifik bir rol talep etmez.
public sealed class GetCandidateDevelopmentSummaryQueryHandler(
    ISkillGapRepository skillGapRepository,
    ICareerGoalRepository careerGoalRepository,
    IDevelopmentPlanRepository developmentPlanRepository,
    ITrainingRecommendationRepository trainingRecommendationRepository,
    IAdvisorRecommendationRepository advisorRecommendationRepository,
    ICandidateModuleContract candidateModuleContract,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<GetCandidateDevelopmentSummaryQuery, Result<GetCandidateDevelopmentSummaryResponse>>
{
    public async Task<Result<GetCandidateDevelopmentSummaryResponse>> Handle(
        GetCandidateDevelopmentSummaryQuery request, CancellationToken cancellationToken)
    {
        var isAuthorized = await IsCallerTheCandidateAsync(request.CandidateCvId, cancellationToken)
            || await IsCallerTheCurrentAdvisorAsync(request.CandidateCvId, cancellationToken);

        if (!isAuthorized)
        {
            return Result.Failure<GetCandidateDevelopmentSummaryResponse>(Error.Forbidden(
                "CareerDevelopment.NotAuthorized", "You may only view your own development summary or your own candidates'."));
        }

        var skillGaps = await skillGapRepository.GetByCandidateCvIdAsync(request.CandidateCvId, cancellationToken);
        var careerGoals = await careerGoalRepository.GetByCandidateCvIdAsync(request.CandidateCvId, cancellationToken);
        var developmentPlans = await developmentPlanRepository.GetByCandidateCvIdAsync(request.CandidateCvId, cancellationToken);
        var trainingRecommendations = await trainingRecommendationRepository.GetByCandidateCvIdAsync(request.CandidateCvId, cancellationToken);
        var advisorRecommendations = await advisorRecommendationRepository.GetByCandidateCvIdAsync(request.CandidateCvId, cancellationToken);

        var response = new GetCandidateDevelopmentSummaryResponse(
            skillGaps.Select(SkillGapItemResponse.FromDomain).ToList(),
            careerGoals.Select(CareerGoalItemResponse.FromDomain).ToList(),
            developmentPlans.Select(DevelopmentPlanItemResponse.FromDomain).ToList(),
            trainingRecommendations.Select(TrainingRecommendationItemResponse.FromDomain).ToList(),
            advisorRecommendations.Select(AdvisorRecommendationItemResponse.FromDomain).ToList());

        return Result.Success(response);
    }

    private async Task<bool> IsCallerTheCandidateAsync(Guid candidateCvId, CancellationToken cancellationToken)
    {
        var candidate = await candidateModuleContract.GetCandidateCvByUserIdAsync(currentUserContext.UserId!.Value, cancellationToken);

        return candidate is not null && candidate.Id == candidateCvId;
    }

    private async Task<bool> IsCallerTheCurrentAdvisorAsync(Guid candidateCvId, CancellationToken cancellationToken)
    {
        var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        if (callerAdvisorId is null)
        {
            return false;
        }

        var currentAdvisorId = await candidateModuleContract.GetCareerAdvisorIdForCandidateAsync(candidateCvId, cancellationToken);

        return currentAdvisorId == callerAdvisorId;
    }
}
