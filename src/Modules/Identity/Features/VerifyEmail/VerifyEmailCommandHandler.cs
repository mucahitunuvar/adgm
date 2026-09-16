using GenclikMerkezi.Modules.Identity;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Identity.Features.VerifyEmail;

public sealed class VerifyEmailCommandHandler(
    IUserRepository userRepository,
    IEmailVerificationTokenGenerator tokenGenerator,
    [FromKeyedServices(IdentityModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<VerifyEmailCommand, Result>
{
    private static readonly Error InvalidVerificationToken =
        Error.Unauthorized("Auth.InvalidVerificationToken", "The email verification token is invalid or has expired.");

    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = tokenGenerator.Hash(request.Token);
        var user = await userRepository.GetByEmailVerificationTokenHashAsync(tokenHash, cancellationToken);

        if (user is null)
        {
            return Result.Failure(InvalidVerificationToken);
        }

        var result = user.ConfirmEmail(tokenHash);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
