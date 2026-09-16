namespace GenclikMerkezi.Modules.Identity.Infrastructure.Security;

public sealed class EmailVerificationSettings
{
    public const string SectionName = "EmailVerification";

    public int TokenExpirationHours { get; init; } = 48;
}
