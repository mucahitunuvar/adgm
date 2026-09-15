using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace GenclikMerkezi.Modules.Identity.Infrastructure.Notifications;

// Placeholder implementation until the Notification module + Outbox pipeline exist (see AGENTS.md §30/§31).
// It never logs the plaintext reset token (AGENTS.md §27/§38); it only records that a reset was requested.
// Replace with a real Outbox-backed email dispatch once Notification module is built.
public sealed class LoggingPasswordResetTokenNotifier(ILogger<LoggingPasswordResetTokenNotifier> logger)
    : IPasswordResetTokenNotifier
{
    public Task NotifyAsync(string email, string resetToken, DateTime expiresAtUtc, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Password reset requested for {Email}, token expires at {ExpiresAtUtc}.",
            email,
            expiresAtUtc);

        return Task.CompletedTask;
    }
}
