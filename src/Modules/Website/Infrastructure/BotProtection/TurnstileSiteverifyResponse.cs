using System.Text.Json.Serialization;

namespace GenclikMerkezi.Modules.Website.Infrastructure.BotProtection;

// Shape of https://challenges.cloudflare.com/turnstile/v0/siteverify's JSON response. Several other
// fields exist (challenge_ts, action, cdata) but nothing here needs them yet.
internal sealed class TurnstileSiteverifyResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("error-codes")]
    public IReadOnlyList<string>? ErrorCodes { get; init; }

    [JsonPropertyName("hostname")]
    public string? Hostname { get; init; }
}
