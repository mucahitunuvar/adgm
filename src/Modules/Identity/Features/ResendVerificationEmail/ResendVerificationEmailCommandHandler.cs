using GenclikMerkezi.Contracts.IntegrationEvents;
using GenclikMerkezi.Modules.Identity;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Identity.Features.ResendVerificationEmail;

public sealed class ResendVerificationEmailCommandHandler(
    IUserRepository userRepository,
    IEmailVerificationTokenGenerator tokenGenerator,
    IIntegrationEventPublisher integrationEventPublisher,
    [FromKeyedServices(IdentityModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ResendVerificationEmailCommand, Result>
{
    private static readonly TimeSpan MinimumResendInterval = TimeSpan.FromMinutes(1);

    public async Task<Result> Handle(ResendVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);

        if (emailResult.IsSuccess)
        {
            var user = await userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);

            if (user is not null)
            {
                var plainToken = tokenGenerator.GenerateToken();
                var tokenHash = tokenGenerator.Hash(plainToken);
                var expiresAtUtc = DateTime.UtcNow.Add(tokenGenerator.Lifetime);

                var issuedToken = user.RequestEmailVerificationResend(tokenHash, expiresAtUtc, MinimumResendInterval);

                if (issuedToken is not null)
                {
                    var integrationEvent = new EmailVerificationRequestedIntegrationEvent(
                        user.Id,
                        user.Email.Value,
                        plainToken,
                        expiresAtUtc,
                        DateTime.UtcNow);

                    await integrationEventPublisher.PublishTransactionalAsync(
                        IntegrationEventTopics.EmailVerificationRequested,
                        integrationEvent,
                        () => unitOfWork.SaveChangesAsync(cancellationToken),
                        cancellationToken);
                }
            }
        }

        // Always succeed - regardless of whether the email is registered, already verified, or
        // still within the one-per-minute resend cooldown - to avoid user enumeration and to keep
        // the cooldown itself unobservable to a caller (same trade-off as ForgotPasswordCommandHandler).
        return Result.Success();
    }
}
