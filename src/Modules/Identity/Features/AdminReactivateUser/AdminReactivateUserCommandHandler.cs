using GenclikMerkezi.Modules.Identity;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenclikMerkezi.Modules.Identity.Features.AdminReactivateUser;

public sealed class AdminReactivateUserCommandHandler(
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    ILogger<AdminReactivateUserCommandHandler> logger,
    [FromKeyedServices(IdentityModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<AdminReactivateUserCommand, Result>
{
    public async Task<Result> Handle(AdminReactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(Error.NotFound("User.NotFound", "The specified user could not be found."));
        }

        user.Reactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Admin {AdminUserId} reactivated account {TargetUserId}", currentUserService.UserId, request.UserId);

        return Result.Success();
    }
}
