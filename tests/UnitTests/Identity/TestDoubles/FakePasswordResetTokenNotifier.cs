using GenclikMerkezi.Modules.Identity.Application.Abstractions;

namespace GenclikMerkezi.UnitTests.Identity.TestDoubles;

public sealed class FakePasswordResetTokenNotifier : IPasswordResetTokenNotifier
{
    public int NotifyCallCount { get; private set; }

    public string? LastEmail { get; private set; }

    public string? LastResetToken { get; private set; }

    public Task NotifyAsync(string email, string resetToken, DateTime expiresAtUtc, CancellationToken cancellationToken = default)
    {
        NotifyCallCount++;
        LastEmail = email;
        LastResetToken = resetToken;
        return Task.CompletedTask;
    }
}
