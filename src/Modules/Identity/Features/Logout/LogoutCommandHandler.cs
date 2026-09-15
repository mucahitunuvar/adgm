using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Identity.Features.Logout;

public sealed class LogoutCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenGenerator refreshTokenGenerator,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = refreshTokenGenerator.Hash(request.RefreshToken);
        var user = await userRepository.GetByRefreshTokenHashAsync(tokenHash, cancellationToken);

        if (user is null)
        {
            return Result.Success();
        }

        if (user.Id != currentUserService.UserId)
        {
            return Result.Failure(Error.Forbidden("Auth.NotYourSession", "This session does not belong to you."));
        }

        user.RevokeRefreshToken(tokenHash);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
