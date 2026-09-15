using GenclikMerkezi.Modules.Identity;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Identity.Features.RefreshAccessToken;

public sealed class RefreshAccessTokenCommandHandler(
    IUserRepository userRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    [FromKeyedServices(IdentityModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<RefreshAccessTokenCommand, Result<LoginResponse>>
{
    private static readonly Error InvalidRefreshToken =
        Error.Unauthorized("Auth.InvalidRefreshToken", "The refresh token is invalid or has expired.");

    public async Task<Result<LoginResponse>> Handle(
        RefreshAccessTokenCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash = refreshTokenGenerator.Hash(request.RefreshToken);
        var user = await userRepository.GetByRefreshTokenHashAsync(tokenHash, cancellationToken);
        var refreshToken = user?.FindRefreshToken(tokenHash);

        if (user is null || refreshToken is null)
        {
            return Result.Failure<LoginResponse>(InvalidRefreshToken);
        }

        if (refreshToken.IsRevoked)
        {
            user.RevokeAllActiveRefreshTokens();
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<LoginResponse>(
                Error.Unauthorized(
                    "Auth.RefreshTokenReuseDetected",
                    "This refresh token has already been used. All sessions have been revoked."));
        }

        if (refreshToken.IsExpired)
        {
            return Result.Failure<LoginResponse>(InvalidRefreshToken);
        }

        user.UnlockIfLockoutExpired();

        if (user.Status != UserStatus.Active)
        {
            return Result.Failure<LoginResponse>(
                Error.Forbidden("Auth.AccountNotActive", "This account is not active."));
        }

        var newRefreshTokenPlainText = refreshTokenGenerator.GenerateToken();
        var newRefreshTokenHash = refreshTokenGenerator.Hash(newRefreshTokenPlainText);
        var newRefreshTokenExpiresAtUtc = DateTime.UtcNow.Add(refreshTokenGenerator.Lifetime);

        user.RevokeRefreshToken(tokenHash, newRefreshTokenHash);
        user.IssueRefreshToken(newRefreshTokenHash, newRefreshTokenExpiresAtUtc);

        var accessToken = jwtTokenGenerator.GenerateAccessToken(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new LoginResponse(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            newRefreshTokenPlainText,
            newRefreshTokenExpiresAtUtc));
    }
}
