using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateDevelopmentPlan;

public sealed record CreateDevelopmentPlanCommand(Guid CandidateCvId, Guid? SkillGapId, Guid? CareerGoalId, string Description)
    : IRequest<Result<CreateDevelopmentPlanResponse>>;
