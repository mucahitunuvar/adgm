using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;
using GenclikMerkezi.Modules.CareerDevelopment.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateTrainingRecommendation;

// CreateDevelopmentPlanCommandHandler'daki modül-içi id doğrulamasının aynısı (DevelopmentPlanId
// için). TrainingId doğrulanmaz - Website.Training'e cross-module, doğrulanmayan referans (ADR-011
// "Önemli sınır").
public sealed class CreateTrainingRecommendationCommandHandler(
    ITrainingRecommendationRepository trainingRecommendationRepository,
    IDevelopmentPlanRepository developmentPlanRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICandidateModuleContract candidateModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(CareerDevelopmentModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateTrainingRecommendationCommand, Result<CreateTrainingRecommendationResponse>>
{
    public async Task<Result<CreateTrainingRecommendationResponse>> Handle(
        CreateTrainingRecommendationCommand request, CancellationToken cancellationToken)
    {
        var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        if (callerAdvisorId is null)
        {
            return Result.Failure<CreateTrainingRecommendationResponse>(Error.Forbidden(
                "TrainingRecommendation.NotACareerAdvisor", "Only an active career advisor may recommend a training."));
        }

        var candidateAdvisorId = await candidateModuleContract.GetCareerAdvisorIdForCandidateAsync(
            request.CandidateCvId, cancellationToken);

        if (candidateAdvisorId != callerAdvisorId)
        {
            return Result.Failure<CreateTrainingRecommendationResponse>(Error.Forbidden(
                "TrainingRecommendation.NotOwnCandidate", "You may only recommend trainings for your own candidates."));
        }

        if (request.DevelopmentPlanId is not null)
        {
            var developmentPlan = await developmentPlanRepository.GetByIdAsync(request.DevelopmentPlanId.Value, cancellationToken);

            if (developmentPlan is null || developmentPlan.CandidateCvId != request.CandidateCvId)
            {
                return Result.Failure<CreateTrainingRecommendationResponse>(Error.Conflict(
                    "TrainingRecommendation.InvalidDevelopmentPlan", "The specified development plan does not exist for this candidate."));
            }
        }

        var trainingRecommendation = TrainingRecommendation.Create(
            request.CandidateCvId, request.DevelopmentPlanId, request.TrainingId, callerAdvisorId.Value, request.Notes, DateTime.UtcNow);

        trainingRecommendationRepository.Add(trainingRecommendation);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateTrainingRecommendationResponse(trainingRecommendation.Id));
    }
}
