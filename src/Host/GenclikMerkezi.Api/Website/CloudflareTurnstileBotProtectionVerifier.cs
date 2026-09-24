using System.Net.Http.Json;
using GenclikMerkezi.Contracts.Website;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.Api.Website;

// Host-level adapter (ADR-024 §1/§12.3 Görev 8): implements Website's IBotProtectionVerifier port
// against Cloudflare Turnstile's siteverify endpoint (Managed mode; no Pre-Clearance cookie is
// used). Registered as a typed HttpClient (Program.cs) whose BaseAddress is
// https://challenges.cloudflare.com/.
//
// Fails closed on any transport/parsing problem (AGENTS.md §27: security checks are enforced
// server-side, not bypassed) - an unreachable or misbehaving Turnstile rejects the submission
// rather than silently letting it through. This deliberately does not distinguish "Cloudflare is
// down" from "the token was invalid": both fail the same way, since accepting an unverifiable
// submission would defeat the point of the check.
public sealed class CloudflareTurnstileBotProtectionVerifier(HttpClient httpClient, IOptions<TurnstileSettings> options)
    : IBotProtectionVerifier
{
    public async Task<Result> VerifyAsync(string? token, string? remoteIpAddress, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Result.Failure(Error.Validation(
                "BotProtection.TokenMissing", "A bot-protection challenge token is required."));
        }

        var formFields = new List<KeyValuePair<string, string>>
        {
            new("secret", options.Value.SecretKey),
            new("response", token),
        };

        if (!string.IsNullOrWhiteSpace(remoteIpAddress))
        {
            formFields.Add(new KeyValuePair<string, string>("remoteip", remoteIpAddress));
        }

        TurnstileSiteverifyResponse? siteverifyResponse;
        try
        {
            using var httpResponse = await httpClient.PostAsync(
                "turnstile/v0/siteverify", new FormUrlEncodedContent(formFields), cancellationToken);
            httpResponse.EnsureSuccessStatusCode();
            siteverifyResponse = await httpResponse.Content.ReadFromJsonAsync<TurnstileSiteverifyResponse>(cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or NotSupportedException or System.Text.Json.JsonException)
        {
            return Result.Failure(Error.Failure(
                "BotProtection.VerificationUnavailable", "Bot protection verification could not be completed."));
        }

        if (siteverifyResponse is { Success: true })
        {
            return Result.Success();
        }

        var errorCodes = siteverifyResponse?.ErrorCodes is { Count: > 0 } codes ? string.Join(", ", codes) : "unknown";
        return Result.Failure(Error.Validation(
            "BotProtection.VerificationFailed", $"Bot protection challenge failed ({errorCodes})."));
    }
}
