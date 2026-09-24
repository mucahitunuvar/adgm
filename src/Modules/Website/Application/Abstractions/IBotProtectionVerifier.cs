using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §1/§12.3: the port Website defines for verifying an anonymous submission's
// bot-protection challenge (form, event registration, newsletter). Unlike IWebsiteEmailSender, this
// port's implementation (TurnstileBotProtectionVerifier, Website.Infrastructure) never touches
// another business module - it only calls Cloudflare, a third-party service - so it does not need a
// Host-level adapter (ARCHITECTURE.md §50.1): it is registered directly by AddWebsiteModule, the
// same way IImageProcessor's SkiaSharp implementation is. A different project can still swap
// providers by registering a different IBotProtectionVerifier, without touching Website's own code.
// Whether a given feature enforces this check at all is controlled by
// SiteSettings.BotProtectionEnabled, checked by the caller - this port only answers "is this
// specific token valid", nothing more.
public interface IBotProtectionVerifier
{
    Task<Result> VerifyAsync(string? token, string? remoteIpAddress, CancellationToken cancellationToken = default);
}
