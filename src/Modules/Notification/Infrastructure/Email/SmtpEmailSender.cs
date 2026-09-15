using GenclikMerkezi.Modules.Notification.Application.Abstractions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GenclikMerkezi.Modules.Notification.Infrastructure.Email;

// Uses MailKit rather than System.Net.Mail.SmtpClient: Microsoft's own docs advise against
// SmtpClient for new development (limited STARTTLS/auth support, no real async I/O).
public sealed class SmtpEmailSender(IOptions<SmtpSettings> options) : IEmailSender
{
    private readonly SmtpSettings _settings = options.Value;

    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();

        var secureSocketOptions = _settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
        await client.ConnectAsync(_settings.Host, _settings.Port, secureSocketOptions, cancellationToken);
        await client.AuthenticateAsync(_settings.UserName, _settings.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
