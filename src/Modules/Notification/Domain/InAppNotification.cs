using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Notification.Domain;

// Minimal by design (Görev 4): no IsRead/read-tracking fields yet since nothing reads this back -
// only INotificationModuleContract writes it today. Extend when an actual "my notifications" inbox
// feature needs it.
public sealed class InAppNotification : AggregateRoot
{
    public Guid UserId { get; private set; }

    public string Message { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private InAppNotification(Guid id, Guid userId, string message, DateTime createdAtUtc)
        : base(id)
    {
        UserId = userId;
        Message = message;
        CreatedAtUtc = createdAtUtc;
    }

    public static InAppNotification Create(Guid userId, string message) =>
        new(Guid.NewGuid(), userId, message, DateTime.UtcNow);
}
