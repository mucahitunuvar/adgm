using GenclikMerkezi.Contracts.Notification;
using GenclikMerkezi.Modules.Notification.Application.Abstractions;
using GenclikMerkezi.Modules.Notification.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Notification.Infrastructure;

public sealed class NotificationModuleContract(
    IEmailSender emailSender,
    IEmailNotificationRepository emailNotificationRepository,
    IInAppNotificationRepository inAppNotificationRepository,
    [FromKeyedServices(NotificationModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : INotificationModuleContract
{
    public async Task SendAsync(
        Guid userId, string recipientEmail, string subject, string message, CancellationToken cancellationToken = default)
    {
        var emailNotification = EmailNotification.Create(recipientEmail, subject, message);

        // Best-effort (ADR-022 §3/§6): failures are recorded, not thrown - same pattern as
        // SendPasswordResetEmailCommandHandler.
        try
        {
            await emailSender.SendAsync(recipientEmail, subject, message, cancellationToken);
            emailNotification.MarkSent();
        }
        catch (Exception ex)
        {
            emailNotification.MarkFailed(ex.Message);
        }

        emailNotificationRepository.Add(emailNotification);

        var inAppNotification = InAppNotification.Create(userId, message);
        inAppNotificationRepository.Add(inAppNotification);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
