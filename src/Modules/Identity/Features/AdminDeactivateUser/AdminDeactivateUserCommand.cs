using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Identity.Features.AdminDeactivateUser;

public sealed record AdminDeactivateUserCommand(Guid UserId) : IRequest<Result>;
