using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.CompleteDevelopmentPlan;

public sealed record CompleteDevelopmentPlanCommand(Guid DevelopmentPlanId) : IRequest<Result>;
