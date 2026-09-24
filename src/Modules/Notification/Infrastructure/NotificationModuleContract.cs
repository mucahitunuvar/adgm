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

    public async Task SendBulkAsync(
        IEnumerable<NotificationRecipient> recipients, string subject, string message, CancellationToken cancellationToken = default)
    {
        foreach (var recipient in recipients)
        {
            var emailNotification = EmailNotification.Create(recipient.Email, subject, message);

            try
            {
                await emailSender.SendAsync(recipient.Email, subject, message, cancellationToken);
                emailNotification.MarkSent();
            }
            catch (Exception ex)
            {
                emailNotification.MarkFailed(ex.Message);
            }

            emailNotificationRepository.Add(emailNotification);

            var inAppNotification = InAppNotification.Create(recipient.UserId, message);
            inAppNotificationRepository.Add(inAppNotification);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SendEmailAsync(
        string recipientEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        var emailNotification = EmailNotification.Create(recipientEmail, subject, body);

        // Best-effort (ADR-022 §3/§6), same as SendAsync/SendBulkAsync - but deliberately no
        // InAppNotification here: there is no UserId to attach one to.
        try
        {
            await emailSender.SendAsync(recipientEmail, subject, body, cancellationToken);
            emailNotification.MarkSent();
        }
        catch (Exception ex)
        {
            emailNotification.MarkFailed(ex.Message);
        }

        emailNotificationRepository.Add(emailNotification);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
