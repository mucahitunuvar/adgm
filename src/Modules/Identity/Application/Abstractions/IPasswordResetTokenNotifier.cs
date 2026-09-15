namespace GenclikMerkezi.Modules.Identity.Application.Abstractions;

public interface IPasswordResetTokenNotifier
{
    Task NotifyAsync(string email, string resetToken, DateTime expiresAtUtc, CancellationToken cancellationToken = default);
}
