using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateAdvisorRecommendation;

public sealed record CreateAdvisorRecommendationCommand(Guid CandidateCvId, string Content)
    : IRequest<Result<CreateAdvisorRecommendationResponse>>;
