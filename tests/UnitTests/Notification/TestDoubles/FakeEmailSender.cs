using GenclikMerkezi.Modules.Notification.Application.Abstractions;

namespace GenclikMerkezi.UnitTests.Notification.TestDoubles;

public sealed class FakeEmailSender : IEmailSender
{
    public List<(string ToEmail, string Subject, string Body)> SentEmails { get; } = [];

    public Exception? ThrowOnSend { get; set; }

    public Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (ThrowOnSend is not null)
        {
            throw ThrowOnSend;
        }

        SentEmails.Add((toEmail, subject, body));
        return Task.CompletedTask;
    }
}
