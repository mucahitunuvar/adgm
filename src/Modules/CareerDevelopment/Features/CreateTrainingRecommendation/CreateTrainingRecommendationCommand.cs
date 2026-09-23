using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateTrainingRecommendation;

public sealed record CreateTrainingRecommendationCommand(Guid CandidateCvId, Guid? DevelopmentPlanId, Guid TrainingId, string? Notes)
    : IRequest<Result<CreateTrainingRecommendationResponse>>;
