using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Identity.Features.AdminReactivateUser;

public sealed record AdminReactivateUserCommand(Guid UserId) : IRequest<Result>;
