using GenclikMerkezi.Modules.Identity;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Identity.Features.ResetPassword;

public sealed class ResetPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordResetTokenGenerator tokenGenerator,
    IPasswordHasher passwordHasher,
    [FromKeyedServices(IdentityModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ResetPasswordCommand, Result>
{
    private static readonly Error InvalidResetToken =
        Error.Unauthorized("Auth.InvalidResetToken", "The password reset token is invalid or has expired.");

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = tokenGenerator.Hash(request.Token);
        var user = await userRepository.GetByPasswordResetTokenHashAsync(tokenHash, cancellationToken);

        if (user is null)
        {
            return Result.Failure(InvalidResetToken);
        }

        var newPasswordHash = PasswordHash.FromHashedValue(passwordHasher.Hash(request.NewPassword));
        var result = user.ResetPassword(tokenHash, newPasswordHash);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
