using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;
using GenclikMerkezi.Modules.CareerDevelopment.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateCareerGoal;

// CreateSkillGapCommandHandler'daki "yalnızca kendi adayı" kontrolünün aynısı.
public sealed class CreateCareerGoalCommandHandler(
    ICareerGoalRepository careerGoalRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICandidateModuleContract candidateModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(CareerDevelopmentModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCareerGoalCommand, Result<CreateCareerGoalResponse>>
{
    public async Task<Result<CreateCareerGoalResponse>> Handle(CreateCareerGoalCommand request, CancellationToken cancellationToken)
    {
        var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        if (callerAdvisorId is null)
        {
            return Result.Failure<CreateCareerGoalResponse>(
                Error.Forbidden("CareerGoal.NotACareerAdvisor", "Only an active career advisor may set a career goal."));
        }

        var candidateAdvisorId = await candidateModuleContract.GetCareerAdvisorIdForCandidateAsync(
            request.CandidateCvId, cancellationToken);

        if (candidateAdvisorId != callerAdvisorId)
        {
            return Result.Failure<CreateCareerGoalResponse>(
                Error.Forbidden("CareerGoal.NotOwnCandidate", "You may only set career goals for your own candidates."));
        }

        var careerGoal = CareerGoal.Create(
            request.CandidateCvId, request.Description, request.TargetPositionId, callerAdvisorId.Value, DateTime.UtcNow);

        careerGoalRepository.Add(careerGoal);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateCareerGoalResponse(careerGoal.Id));
    }
}
