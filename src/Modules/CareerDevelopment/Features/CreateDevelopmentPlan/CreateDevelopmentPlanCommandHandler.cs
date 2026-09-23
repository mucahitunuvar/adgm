using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;
using GenclikMerkezi.Modules.CareerDevelopment.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateDevelopmentPlan;

// CreateSkillGapCommandHandler'daki "yalnızca kendi adayı" kontrolüne ek olarak, SkillGapId/
// CareerGoalId verilmişse aynı modül içi repository'lerden var olup olmadıkları VE aynı adaya ait
// oldukları doğrulanır (TrainingId'nin aksine bunlar cross-module değil, doğrulanabilir) - mevcut
// olmama ve başka adaya ait olma durumları tek bir Conflict'te birleştirilir (RequestInterviewAsEmployer
// Command Handler'ın "candidate not found"/"candidate has no advisor" sadeleştirmesiyle aynı desen).
public sealed class CreateDevelopmentPlanCommandHandler(
    IDevelopmentPlanRepository developmentPlanRepository,
    ISkillGapRepository skillGapRepository,
    ICareerGoalRepository careerGoalRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICandidateModuleContract candidateModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(CareerDevelopmentModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateDevelopmentPlanCommand, Result<CreateDevelopmentPlanResponse>>
{
    public async Task<Result<CreateDevelopmentPlanResponse>> Handle(
        CreateDevelopmentPlanCommand request, CancellationToken cancellationToken)
    {
        var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        if (callerAdvisorId is null)
        {
            return Result.Failure<CreateDevelopmentPlanResponse>(
                Error.Forbidden("DevelopmentPlan.NotACareerAdvisor", "Only an active career advisor may create a development plan."));
        }

        var candidateAdvisorId = await candidateModuleContract.GetCareerAdvisorIdForCandidateAsync(
            request.CandidateCvId, cancellationToken);

        if (candidateAdvisorId != callerAdvisorId)
        {
            return Result.Failure<CreateDevelopmentPlanResponse>(Error.Forbidden(
                "DevelopmentPlan.NotOwnCandidate", "You may only create development plans for your own candidates."));
        }

        if (request.SkillGapId is not null)
        {
            var skillGap = await skillGapRepository.GetByIdAsync(request.SkillGapId.Value, cancellationToken);

            if (skillGap is null || skillGap.CandidateCvId != request.CandidateCvId)
            {
                return Result.Failure<CreateDevelopmentPlanResponse>(Error.Conflict(
                    "DevelopmentPlan.InvalidSkillGap", "The specified skill gap does not exist for this candidate."));
            }
        }

        if (request.CareerGoalId is not null)
        {
            var careerGoal = await careerGoalRepository.GetByIdAsync(request.CareerGoalId.Value, cancellationToken);

            if (careerGoal is null || careerGoal.CandidateCvId != request.CandidateCvId)
            {
                return Result.Failure<CreateDevelopmentPlanResponse>(Error.Conflict(
                    "DevelopmentPlan.InvalidCareerGoal", "The specified career goal does not exist for this candidate."));
            }
        }

        var developmentPlan = DevelopmentPlan.Create(
            request.CandidateCvId, request.SkillGapId, request.CareerGoalId, request.Description, callerAdvisorId.Value, DateTime.UtcNow);

        developmentPlanRepository.Add(developmentPlan);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateDevelopmentPlanResponse(developmentPlan.Id));
    }
}
