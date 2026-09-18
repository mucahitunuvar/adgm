using GenclikMerkezi.Contracts.Notification;

namespace GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;

public sealed class FakeNotificationModuleContract : INotificationModuleContract
{
    private readonly List<(Guid UserId, string RecipientEmail, string Subject, string Message)> _sentNotifications = [];

    public IReadOnlyCollection<(Guid UserId, string RecipientEmail, string Subject, string Message)> SentNotifications =>
        _sentNotifications.AsReadOnly();

    public Task SendAsync(
        Guid userId, string recipientEmail, string subject, string message, CancellationToken cancellationToken = default)
    {
        _sentNotifications.Add((userId, recipientEmail, subject, message));
        return Task.CompletedTask;
    }
}
