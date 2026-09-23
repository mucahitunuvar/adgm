using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CreateCareerGoal;

public sealed record CreateCareerGoalCommand(Guid CandidateCvId, string Description, Guid? TargetPositionId)
    : IRequest<Result<CreateCareerGoalResponse>>;
