using GenclikMerkezi.Contracts.Website;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeWebsiteEmailSender : IWebsiteEmailSender
{
    public List<(string RecipientEmail, string Subject, string HtmlBody)> SentEmails { get; } = [];

    public Task SendEmailAsync(string recipientEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        SentEmails.Add((recipientEmail, subject, htmlBody));
        return Task.CompletedTask;
    }
}
