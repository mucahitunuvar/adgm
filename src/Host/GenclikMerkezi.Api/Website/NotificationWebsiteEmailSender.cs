using GenclikMerkezi.Contracts.Notification;
using GenclikMerkezi.Contracts.Website;

namespace GenclikMerkezi.Api.Website;

// Host-level adapter (ADR-024 §1 Görev 7): implements Website's IWebsiteEmailSender port by
// forwarding to Notification's public contract. Website itself never references
// INotificationModuleContract, or any other module - only the Host (the composition root, and not
// itself a "module" under ModuleBoundaryTests) is allowed to know about both sides of this wiring.
// A different project can swap this one adapter for a different provider without Website changing.
public sealed class NotificationWebsiteEmailSender(INotificationModuleContract notificationModuleContract) : IWebsiteEmailSender
{
    public Task SendEmailAsync(string recipientEmail, string subject, string htmlBody, CancellationToken cancellationToken = default) =>
        notificationModuleContract.SendEmailAsync(recipientEmail, subject, htmlBody, cancellationToken);
}
