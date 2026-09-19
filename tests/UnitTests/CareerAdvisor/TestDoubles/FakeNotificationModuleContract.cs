using GenclikMerkezi.Contracts.Notification;

namespace GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;

public sealed class FakeNotificationModuleContract : INotificationModuleContract
{
    private readonly List<(Guid UserId, string RecipientEmail, string Subject, string Message)> _sentNotifications = [];
    private readonly List<(IReadOnlyList<NotificationRecipient> Recipients, string Subject, string Message)> _sentBulkNotifications = [];

    public IReadOnlyCollection<(Guid UserId, string RecipientEmail, string Subject, string Message)> SentNotifications =>
        _sentNotifications.AsReadOnly();

    public IReadOnlyCollection<(IReadOnlyList<NotificationRecipient> Recipients, string Subject, string Message)> SentBulkNotifications =>
        _sentBulkNotifications.AsReadOnly();

    public Task SendAsync(
        Guid userId, string recipientEmail, string subject, string message, CancellationToken cancellationToken = default)
    {
        _sentNotifications.Add((userId, recipientEmail, subject, message));
        return Task.CompletedTask;
    }

    public Task SendBulkAsync(
        IEnumerable<NotificationRecipient> recipients, string subject, string message, CancellationToken cancellationToken = default)
    {
        _sentBulkNotifications.Add((recipients.ToList(), subject, message));
        return Task.CompletedTask;
    }
}
