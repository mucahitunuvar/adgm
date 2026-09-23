using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CancelDevelopmentPlan;

public sealed record CancelDevelopmentPlanCommand(Guid DevelopmentPlanId) : IRequest<Result>;
