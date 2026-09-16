using GenclikMerkezi.Modules.Notification.Application.Abstractions;
using GenclikMerkezi.Modules.Notification.Domain;

namespace GenclikMerkezi.UnitTests.Notification.TestDoubles;

public sealed class FakeEmailNotificationRepository : IEmailNotificationRepository
{
    private readonly List<EmailNotification> _notifications = [];

    public IReadOnlyCollection<EmailNotification> Notifications => _notifications.AsReadOnly();

    public void Add(EmailNotification emailNotification)
    {
        _notifications.Add(emailNotification);
    }
}
