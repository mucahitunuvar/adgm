using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Contracts.Website;

// ADR-024 §1/§12.3: the port Website defines for verifying an anonymous submission's
// bot-protection challenge (form, event registration, newsletter). Website itself never references
// Cloudflare or any HTTP client directly; this project's Host adapter
// (CloudflareTurnstileBotProtectionVerifier) implements it against Cloudflare Turnstile, and a
// different project could swap in a different provider without Website's code changing. Whether a
// given feature enforces this check at all is controlled by SiteSettings.BotProtectionEnabled,
// checked by the caller - this port only answers "is this specific token valid", nothing more.
public interface IBotProtectionVerifier
{
    Task<Result> VerifyAsync(string? token, string? remoteIpAddress, CancellationToken cancellationToken = default);
}
