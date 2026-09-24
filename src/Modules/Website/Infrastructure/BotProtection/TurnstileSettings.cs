namespace GenclikMerkezi.Modules.Website.Infrastructure.BotProtection;

public sealed class TurnstileSettings
{
    public const string SectionName = "Website:BotProtection";

    // Never set via appsettings.json (committed). Development: dotnet user-secrets or the
    // gitignored appsettings.Development.json. Production: the
    // Website__BotProtection__TurnstileSecretKey environment variable. See
    // appsettings.Development.json.example. The site key (public, used by the frontend widget) is
    // stored on SiteSettings itself (TurnstileSiteKey) - only the secret key, which must never leave
    // the server, lives here.
    public string TurnstileSecretKey { get; init; } = string.Empty;

    // Cloudflare's siteverify response echoes back the hostname the widget was rendered on - an
    // attacker who obtains a valid token for their own site cannot replay it against this one unless
    // their hostname is also listed here.
    public IReadOnlyList<string> AllowedHostnames { get; init; } = [];
}
