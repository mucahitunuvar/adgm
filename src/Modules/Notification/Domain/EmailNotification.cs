using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Notification.Domain;

public sealed class EmailNotification : AggregateRoot
{
    public string ToEmail { get; private set; }

    public string Subject { get; private set; }

    public string Body { get; private set; }

    public EmailNotificationStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? SentAtUtc { get; private set; }

    public string? FailureReason { get; private set; }

    private EmailNotification(Guid id, string toEmail, string subject, string body)
        : base(id)
    {
        ToEmail = toEmail;
        Subject = subject;
        Body = body;
        Status = EmailNotificationStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private EmailNotification()
    {
        ToEmail = string.Empty;
        Subject = string.Empty;
        Body = string.Empty;
    }

    public static EmailNotification Create(string toEmail, string subject, string body)
    {
        return new EmailNotification(Guid.NewGuid(), toEmail, subject, body);
    }

    public void MarkSent()
    {
        Status = EmailNotificationStatus.Sent;
        SentAtUtc = DateTime.UtcNow;
        FailureReason = null;
    }

    public void MarkFailed(string reason)
    {
        Status = EmailNotificationStatus.Failed;
        FailureReason = reason;
    }
}
