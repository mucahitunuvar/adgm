using GenclikMerkezi.Modules.Notification.Application.Abstractions;
using GenclikMerkezi.Modules.Notification.Domain;

namespace GenclikMerkezi.Modules.Notification.Infrastructure.Persistence;

public sealed class EmailNotificationRepository(NotificationDbContext dbContext) : IEmailNotificationRepository
{
    public void Add(EmailNotification emailNotification)
    {
        dbContext.EmailNotifications.Add(emailNotification);
    }
}
