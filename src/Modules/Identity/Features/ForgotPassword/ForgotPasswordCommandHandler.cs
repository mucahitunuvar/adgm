using GenclikMerkezi.Contracts.IntegrationEvents;
using GenclikMerkezi.Modules.Identity;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Identity.Features.ForgotPassword;

public sealed class ForgotPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordResetTokenGenerator tokenGenerator,
    IIntegrationEventPublisher integrationEventPublisher,
    [FromKeyedServices(IdentityModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ForgotPasswordCommand, Result>
{
    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);

        if (emailResult.IsSuccess)
        {
            var user = await userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);

            if (user is not null && user.Status == UserStatus.Active)
            {
                var plainToken = tokenGenerator.GenerateToken();
                var tokenHash = tokenGenerator.Hash(plainToken);
                var expiresAtUtc = DateTime.UtcNow.Add(tokenGenerator.Lifetime);

                user.IssuePasswordResetToken(tokenHash, expiresAtUtc);

                var integrationEvent = new PasswordResetRequestedIntegrationEvent(
                    user.Id,
                    user.Email.Value,
                    plainToken,
                    expiresAtUtc,
                    DateTime.UtcNow);

                await integrationEventPublisher.PublishTransactionalAsync(
                    IntegrationEventTopics.PasswordResetRequested,
                    integrationEvent,
                    () => unitOfWork.SaveChangesAsync(cancellationToken),
                    cancellationToken);
            }
        }

        // Always succeed, regardless of whether the email is registered, to avoid user enumeration.
        return Result.Success();
    }
}
