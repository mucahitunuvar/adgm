using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Identity.Features.ChangeUserRole;

public sealed record ChangeUserRoleCommand(Guid UserId, string NewRole) : IRequest<Result>;
