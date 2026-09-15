namespace GenclikMerkezi.Modules.Identity.Infrastructure.Security;

public sealed class PasswordResetSettings
{
    public const string SectionName = "PasswordReset";

    public int TokenExpirationMinutes { get; init; } = 30;
}
