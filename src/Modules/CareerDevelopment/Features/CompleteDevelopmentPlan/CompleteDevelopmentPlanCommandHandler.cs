using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CompleteDevelopmentPlan;

// EndEmploymentCommandHandler'daki taze-danışman kontrolünün aynısı: adayın CreatedByAdvisorId'sine
// değil, ICandidateModuleContract'tan TAZE çözülen GÜNCEL danışmanına göre yetkilendirilir - danışman
// değişmiş olabilir.
public sealed class CompleteDevelopmentPlanCommandHandler(
    IDevelopmentPlanRepository developmentPlanRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICandidateModuleContract candidateModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(CareerDevelopmentModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CompleteDevelopmentPlanCommand, Result>
{
    public async Task<Result> Handle(CompleteDevelopmentPlanCommand request, CancellationToken cancellationToken)
    {
        var developmentPlan = await developmentPlanRepository.GetByIdAsync(request.DevelopmentPlanId, cancellationToken);

        if (developmentPlan is null)
        {
            return Result.Failure(Error.NotFound("DevelopmentPlan.NotFound", "The specified development plan could not be found."));
        }

        var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        var currentAdvisorId = await candidateModuleContract.GetCareerAdvisorIdForCandidateAsync(
            developmentPlan.CandidateCvId, cancellationToken);

        if (callerAdvisorId is null || callerAdvisorId != currentAdvisorId)
        {
            return Result.Failure(Error.Forbidden(
                "DevelopmentPlan.NotCurrentAdvisor", "Only the candidate's current career advisor may complete this development plan."));
        }

        var completeResult = developmentPlan.Complete(DateTime.UtcNow);

        if (completeResult.IsFailure)
        {
            return completeResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
