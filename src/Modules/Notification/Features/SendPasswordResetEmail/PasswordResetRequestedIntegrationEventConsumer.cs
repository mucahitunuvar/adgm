using DotNetCore.CAP;
using GenclikMerkezi.Contracts.IntegrationEvents;
using MediatR;

namespace GenclikMerkezi.Modules.Notification.Features.SendPasswordResetEmail;

// Thin adapter: translates the integration event into an application command and hands it to
// MediatR, keeping the actual work (and its unit tests) in the ordinary CQRS handler above.
public sealed class PasswordResetRequestedIntegrationEventConsumer(ISender sender) : ICapSubscribe
{
    [CapSubscribe(IntegrationEventTopics.PasswordResetRequested)]
    public async Task HandleAsync(PasswordResetRequestedIntegrationEvent integrationEvent)
    {
        await sender.Send(new SendPasswordResetEmailCommand(
            integrationEvent.UserId,
            integrationEvent.Email,
            integrationEvent.ResetToken,
            integrationEvent.ExpiresAtUtc));
    }
}
