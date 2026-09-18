using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.DeactivateCareerAdvisor;

public sealed record DeactivateCareerAdvisorCommand(Guid CareerAdvisorId) : IRequest<Result>;
