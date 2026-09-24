namespace GenclikMerkezi.Api.Website;

public sealed class TurnstileSettings
{
    public const string SectionName = "Turnstile";

    // Never set via appsettings.json (committed). Development: dotnet user-secrets or the
    // gitignored appsettings.Development.json. Production: the Turnstile__SecretKey environment
    // variable. See appsettings.Development.json.example. The site key (public, used by the
    // frontend widget) is not needed here - only the secret key is ever sent to Cloudflare.
    public string SecretKey { get; init; } = string.Empty;
}
