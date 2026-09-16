using GenclikMerkezi.Modules.Identity;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenclikMerkezi.Modules.Identity.Features.AdminUnlockUser;

public sealed class AdminUnlockUserCommandHandler(
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    ILogger<AdminUnlockUserCommandHandler> logger,
    [FromKeyedServices(IdentityModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<AdminUnlockUserCommand, Result>
{
    public async Task<Result> Handle(AdminUnlockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(Error.NotFound("User.NotFound", "The specified user could not be found."));
        }

        user.ManuallyUnlock();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Interim audit trail (AGENTS.md §38/SECURITY.md §8: admin actions must be auditable) - a
        // structured log entry, not yet a durable audit-log store; see the completion report for
        // why a dedicated store is tracked as separate follow-up work rather than added here.
        logger.LogInformation(
            "Admin {AdminUserId} unlocked account {TargetUserId}", currentUserService.UserId, request.UserId);

        return Result.Success();
    }
}
