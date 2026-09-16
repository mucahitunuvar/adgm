using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Identity.Features.AdminUnlockUser;

public sealed record AdminUnlockUserCommand(Guid UserId) : IRequest<Result>;
