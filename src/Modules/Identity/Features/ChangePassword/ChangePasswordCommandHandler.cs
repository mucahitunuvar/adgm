using GenclikMerkezi.Modules.Identity;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Identity.Features.ChangePassword;

public sealed class ChangePasswordCommandHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    [FromKeyedServices(IdentityModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ChangePasswordCommand, Result>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (currentUserService.UserId is not { } userId)
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "No authenticated user found."));
        }

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(Error.NotFound("User.NotFound", "The current user could not be found."));
        }

        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash.Value))
        {
            return Result.Failure(Error.Unauthorized("Auth.InvalidCredentials", "The current password is incorrect."));
        }

        var newPasswordHash = PasswordHash.FromHashedValue(passwordHasher.Hash(request.NewPassword));
        user.ChangePassword(newPasswordHash);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
