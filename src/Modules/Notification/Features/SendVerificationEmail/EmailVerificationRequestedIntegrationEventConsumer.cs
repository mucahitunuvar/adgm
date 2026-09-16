using DotNetCore.CAP;
using GenclikMerkezi.Contracts.IntegrationEvents;
using MediatR;

namespace GenclikMerkezi.Modules.Notification.Features.SendVerificationEmail;

// Thin adapter: translates the integration event into an application command and hands it to
// MediatR, keeping the actual work (and its unit tests) in the ordinary CQRS handler above.
public sealed class EmailVerificationRequestedIntegrationEventConsumer(ISender sender) : ICapSubscribe
{
    [CapSubscribe(IntegrationEventTopics.EmailVerificationRequested)]
    public async Task HandleAsync(EmailVerificationRequestedIntegrationEvent integrationEvent)
    {
        await sender.Send(new SendVerificationEmailCommand(
            integrationEvent.UserId,
            integrationEvent.Email,
            integrationEvent.VerificationToken,
            integrationEvent.ExpiresAtUtc));
    }
}
