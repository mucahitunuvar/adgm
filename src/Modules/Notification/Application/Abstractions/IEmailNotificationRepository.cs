using GenclikMerkezi.Modules.Notification.Domain;

namespace GenclikMerkezi.Modules.Notification.Application.Abstractions;

public interface IEmailNotificationRepository
{
    void Add(EmailNotification emailNotification);
}
