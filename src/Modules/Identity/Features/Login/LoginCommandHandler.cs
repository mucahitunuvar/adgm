using GenclikMerkezi.Modules.Identity;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Identity.Features.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    [FromKeyedServices(IdentityModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private static readonly Error InvalidCredentials =
        Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);

        if (emailResult.IsFailure)
        {
            return Result.Failure<LoginResponse>(InvalidCredentials);
        }

        var user = await userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);

        if (user is null)
        {
            return Result.Failure<LoginResponse>(InvalidCredentials);
        }

        user.UnlockIfLockoutExpired();

        if (user.IsLockedOut)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<LoginResponse>(
                Error.Forbidden(
                    "Auth.AccountLocked",
                    $"This account is temporarily locked until {user.LockedUntilUtc:O} due to too many failed login attempts."));
        }

        if (!passwordHasher.Verify(request.Password, user.PasswordHash.Value))
        {
            user.RegisterFailedLoginAttempt();
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<LoginResponse>(InvalidCredentials);
        }

        if (user.Status != UserStatus.Active)
        {
            return Result.Failure<LoginResponse>(
                Error.Forbidden("Auth.AccountNotActive", "This account is not active."));
        }

        user.RegisterSuccessfulLogin();

        var accessToken = jwtTokenGenerator.GenerateAccessToken(user);

        var refreshTokenPlainText = refreshTokenGenerator.GenerateToken();
        var refreshTokenHash = refreshTokenGenerator.Hash(refreshTokenPlainText);
        var refreshTokenExpiresAtUtc = DateTime.UtcNow.Add(refreshTokenGenerator.Lifetime);

        user.IssueRefreshToken(refreshTokenHash, refreshTokenExpiresAtUtc);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new LoginResponse(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            refreshTokenPlainText,
            refreshTokenExpiresAtUtc));
    }
}
