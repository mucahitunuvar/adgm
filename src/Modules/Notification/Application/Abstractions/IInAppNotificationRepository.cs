using GenclikMerkezi.Modules.Notification.Domain;

namespace GenclikMerkezi.Modules.Notification.Application.Abstractions;

public interface IInAppNotificationRepository
{
    void Add(InAppNotification inAppNotification);
}
