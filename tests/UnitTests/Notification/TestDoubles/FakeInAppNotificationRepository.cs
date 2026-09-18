using GenclikMerkezi.Modules.Notification.Application.Abstractions;
using GenclikMerkezi.Modules.Notification.Domain;

namespace GenclikMerkezi.UnitTests.Notification.TestDoubles;

public sealed class FakeInAppNotificationRepository : IInAppNotificationRepository
{
    private readonly List<InAppNotification> _notifications = [];

    public IReadOnlyCollection<InAppNotification> Notifications => _notifications.AsReadOnly();

    public void Add(InAppNotification inAppNotification)
    {
        _notifications.Add(inAppNotification);
    }
}
