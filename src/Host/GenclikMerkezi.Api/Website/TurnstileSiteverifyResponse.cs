using System.Text.Json.Serialization;

namespace GenclikMerkezi.Api.Website;

// Shape of https://challenges.cloudflare.com/turnstile/v0/siteverify's JSON response. Several other
// fields exist (challenge_ts, hostname, action, cdata) but nothing here needs them yet.
internal sealed class TurnstileSiteverifyResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("error-codes")]
    public IReadOnlyList<string>? ErrorCodes { get; init; }
}
