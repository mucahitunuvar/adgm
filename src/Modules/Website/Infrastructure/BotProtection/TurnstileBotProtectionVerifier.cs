using System.Net.Http.Json;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.Modules.Website.Infrastructure.BotProtection;

// ADR-024 §1/§12.3 Görev 8: implements IBotProtectionVerifier against Cloudflare Turnstile's
// siteverify endpoint (Managed mode; no Pre-Clearance cookie is used). Lives in Website's own
// Infrastructure - not a Host-level adapter - because it never touches another business module, only
// a third-party service (ARCHITECTURE.md §50.1); registered as a typed HttpClient by
// AddWebsiteModule, the same way SkiaSharpImageProcessor is.
//
// Fails closed on any transport/parsing problem, an unsuccessful challenge, or a hostname outside
// the configured allow-list (AGENTS.md §27: security checks are enforced server-side, not bypassed)
// - an unreachable/misbehaving Turnstile, or a token issued for a different site, rejects the
// submission rather than silently letting it through. The secret key is read only from
// TurnstileSettings (bound from configuration) and is never logged or persisted anywhere in this
// class.
public sealed class TurnstileBotProtectionVerifier(HttpClient httpClient, IOptions<TurnstileSettings> options)
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
            new("secret", options.Value.TurnstileSecretKey),
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

        if (siteverifyResponse is not { Success: true })
        {
            var errorCodes = siteverifyResponse?.ErrorCodes is { Count: > 0 } codes ? string.Join(", ", codes) : "unknown";
            return Result.Failure(Error.Validation(
                "BotProtection.VerificationFailed", $"Bot protection challenge failed ({errorCodes})."));
        }

        if (!IsAllowedHostname(siteverifyResponse.Hostname))
        {
            return Result.Failure(Error.Validation(
                "BotProtection.HostnameNotAllowed", "Bot protection challenge was issued for an unrecognized hostname."));
        }

        return Result.Success();
    }

    private bool IsAllowedHostname(string? hostname) =>
        !string.IsNullOrWhiteSpace(hostname)
        && options.Value.AllowedHostnames.Any(allowed => string.Equals(allowed, hostname, StringComparison.OrdinalIgnoreCase));
}
