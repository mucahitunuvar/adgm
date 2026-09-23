using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;
using GenclikMerkezi.Modules.CareerDevelopment.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateAdvisorRecommendation;

// CreateSkillGapCommandHandler'daki "yalnızca kendi adayı" kontrolünün aynısı.
public sealed class CreateAdvisorRecommendationCommandHandler(
    IAdvisorRecommendationRepository advisorRecommendationRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICandidateModuleContract candidateModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(CareerDevelopmentModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAdvisorRecommendationCommand, Result<CreateAdvisorRecommendationResponse>>
{
    public async Task<Result<CreateAdvisorRecommendationResponse>> Handle(
        CreateAdvisorRecommendationCommand request, CancellationToken cancellationToken)
    {
        var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        if (callerAdvisorId is null)
        {
            return Result.Failure<CreateAdvisorRecommendationResponse>(
                Error.Forbidden("AdvisorRecommendation.NotACareerAdvisor", "Only an active career advisor may add a recommendation."));
        }

        var candidateAdvisorId = await candidateModuleContract.GetCareerAdvisorIdForCandidateAsync(
            request.CandidateCvId, cancellationToken);

        if (candidateAdvisorId != callerAdvisorId)
        {
            return Result.Failure<CreateAdvisorRecommendationResponse>(Error.Forbidden(
                "AdvisorRecommendation.NotOwnCandidate", "You may only add recommendations for your own candidates."));
        }

        var advisorRecommendation = AdvisorRecommendation.Create(
            request.CandidateCvId, callerAdvisorId.Value, request.Content, DateTime.UtcNow);

        advisorRecommendationRepository.Add(advisorRecommendation);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateAdvisorRecommendationResponse(advisorRecommendation.Id));
    }
}
