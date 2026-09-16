using System.Collections.Concurrent;
using GenclikMerkezi.Modules.Notification.Application.Abstractions;

namespace GenclikMerkezi.IntegrationTests.Identity;

// Replaces the real SmtpEmailSender for integration tests (no real mail server involved) while
// still exercising the real Register/ForgotPassword -> Outbox -> CAP -> consumer pipeline end to
// end. Captured bodies contain the real verification/reset link, so tests can extract the token
// exactly as a user's mail client would present it - not a backdoor into the token itself.
public sealed class FakeEmailSender : IEmailSender
{
    private readonly ConcurrentBag<(string ToEmail, string Subject, string Body)> _sentEmails = [];

    public IReadOnlyCollection<(string ToEmail, string Subject, string Body)> SentEmails => _sentEmails.ToList();

    public Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        _sentEmails.Add((toEmail, subject, body));
        return Task.CompletedTask;
    }
}
