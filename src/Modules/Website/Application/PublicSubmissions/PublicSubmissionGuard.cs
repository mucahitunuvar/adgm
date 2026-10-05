using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace GenclikMerkezi.Modules.Website.Application.PublicSubmissions;

// ADR-024 §12.3 (Faz 3 Görev 1): checks, in order, (1) the submission token is valid and unexpired,
// (2) at least MinimumFillDuration has passed since it was issued, (3) the honeypot field is empty,
// and (4) when SiteSettings.BotProtectionEnabled is on, the Turnstile challenge. Every failure - no
// matter which step - returns the same detail-free RejectedError (AGENTS.md §27: never leak why an
// anonymous write was rejected); only the structured log says which check actually failed, and never
// logs the token/challenge values themselves.
public sealed class PublicSubmissionGuard(
    ISubmissionTokenGenerator submissionTokenGenerator,
    IBotProtectionVerifier botProtectionVerifier,
    ISiteSettingsRepository siteSettingsRepository,
    TimeProvider timeProvider,
    ILogger<PublicSubmissionGuard> logger)
    : IPublicSubmissionGuard
{
    public static readonly TimeSpan MinimumFillDuration = TimeSpan.FromSeconds(3);

    private static readonly Error RejectedError = Error.Validation(
        "PublicSubmission.Rejected", "This submission could not be processed.");

    public async Task<Result> VerifyAsync(
        PublicSubmissionGuardRequest request, string? remoteIpAddress, CancellationToken cancellationToken = default)
    {
        var tokenResult = submissionTokenGenerator.ValidateToken(request.SubmissionToken);
        if (tokenResult.IsFailure)
        {
            logger.LogWarning("Public submission rejected: submission token is missing, invalid or expired.");
            return Result.Failure(RejectedError);
        }

        var elapsedSinceIssued = timeProvider.GetUtcNow() - tokenResult.Value;
        if (elapsedSinceIssued < MinimumFillDuration)
        {
            logger.LogWarning(
                "Public submission rejected: submitted {ElapsedMilliseconds} ms after its token was issued, below the {MinimumMilliseconds} ms minimum.",
                elapsedSinceIssued.TotalMilliseconds, MinimumFillDuration.TotalMilliseconds);
            return Result.Failure(RejectedError);
        }

        if (!string.IsNullOrEmpty(request.Website))
        {
            logger.LogWarning("Public submission rejected: honeypot field was filled in.");
            return Result.Failure(RejectedError);
        }

        var settings = await siteSettingsRepository.GetAsync(cancellationToken) ?? SiteSettings.CreateDefault();
        if (settings.BotProtectionEnabled)
        {
            var botProtectionResult = await botProtectionVerifier.VerifyAsync(request.TurnstileToken, remoteIpAddress, cancellationToken);
            if (botProtectionResult.IsFailure)
            {
                logger.LogWarning(
                    "Public submission rejected: bot protection challenge failed ({ErrorCode}).", botProtectionResult.Error.Code);
                return Result.Failure(RejectedError);
            }
        }

        return Result.Success();
    }
}
