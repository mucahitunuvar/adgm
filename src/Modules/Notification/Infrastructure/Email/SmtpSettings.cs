namespace GenclikMerkezi.Modules.Notification.Infrastructure.Email;

public sealed class SmtpSettings
{
    public const string SectionName = "Smtp";

    public string Host { get; init; } = string.Empty;

    public int Port { get; init; } = 587;

    public bool EnableSsl { get; init; } = true;

    public string UserName { get; init; } = string.Empty;

    // Never set via appsettings.json (committed). Development: dotnet user-secrets or the
    // gitignored appsettings.Development.json. Production: the Smtp__Password environment
    // variable. See appsettings.Development.json.example.
    public string Password { get; init; } = string.Empty;

    public string FromAddress { get; init; } = string.Empty;

    public string FromName { get; init; } = string.Empty;
}
