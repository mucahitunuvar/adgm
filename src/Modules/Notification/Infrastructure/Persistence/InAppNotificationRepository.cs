using GenclikMerkezi.Modules.Notification.Application.Abstractions;
using GenclikMerkezi.Modules.Notification.Domain;

namespace GenclikMerkezi.Modules.Notification.Infrastructure.Persistence;

public sealed class InAppNotificationRepository(NotificationDbContext dbContext) : IInAppNotificationRepository
{
    public void Add(InAppNotification inAppNotification)
    {
        dbContext.InAppNotifications.Add(inAppNotification);
    }
}
