using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.PublicSubmissions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace GenclikMerkezi.UnitTests.Website.Application.PublicSubmissions;

// ADR-024 §12.3 (Faz 3 Görev 1): every success/failure path through the four checks
// IPublicSubmissionGuard runs in order - token validity, minimum fill time, honeypot, and (only when
// SiteSettings.BotProtectionEnabled) the Turnstile challenge. Every failure must surface as the same
// "PublicSubmission.Rejected" error, regardless of which step actually failed (AGENTS.md §27).
public class PublicSubmissionGuardTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeSubmissionTokenGenerator _submissionTokenGenerator = new();
    private readonly FakeBotProtectionVerifier _botProtectionVerifier = new();
    private readonly FakeSiteSettingsRepository _siteSettingsRepository = new();
    private readonly FakeTimeProvider _timeProvider = new(Now);

    private PublicSubmissionGuard CreateGuard() => new(
        _submissionTokenGenerator, _botProtectionVerifier, _siteSettingsRepository, _timeProvider,
        NullLogger<PublicSubmissionGuard>.Instance);

    private void SeedSiteSettings(bool botProtectionEnabled)
    {
        var settings = SiteSettings.CreateDefault();
        settings.UpdateBotProtection(botProtectionEnabled, "site-key", UserId, Now.UtcDateTime);
        _siteSettingsRepository.Seed(settings);
    }

    private static PublicSubmissionGuardRequest ValidRequest(string? turnstileToken = "turnstile-token") =>
        new("submission-token", turnstileToken, Website: null);

    [Fact]
    public async Task VerifyAsync_AllChecksPass_BotProtectionDisabled_ReturnsSuccessAndSkipsTurnstile()
    {
        _submissionTokenGenerator.NextValidateTokenResult = Result.Success(Now.AddSeconds(-5));
        SeedSiteSettings(botProtectionEnabled: false);

        var result = await CreateGuard().VerifyAsync(ValidRequest(), "203.0.113.1");

        Assert.True(result.IsSuccess);
        Assert.Empty(_botProtectionVerifier.VerifiedTokens);
    }

    [Fact]
    public async Task VerifyAsync_AllChecksPass_BotProtectionEnabledAndTurnstileSucceeds_ReturnsSuccess()
    {
        _submissionTokenGenerator.NextValidateTokenResult = Result.Success(Now.AddSeconds(-5));
        SeedSiteSettings(botProtectionEnabled: true);
        _botProtectionVerifier.ShouldSucceed = true;

        var result = await CreateGuard().VerifyAsync(ValidRequest("turnstile-token"), "203.0.113.1");

        Assert.True(result.IsSuccess);
        Assert.Contains(_botProtectionVerifier.VerifiedTokens, t => t.Token == "turnstile-token" && t.RemoteIpAddress == "203.0.113.1");
    }

    [Fact]
    public async Task VerifyAsync_NoSiteSettingsRowPersisted_DefaultsToBotProtectionEnabled()
    {
        _submissionTokenGenerator.NextValidateTokenResult = Result.Success(Now.AddSeconds(-5));
        // No Seed() call - repository returns null, guard must fall back to SiteSettings.CreateDefault(),
        // which starts with BotProtectionEnabled = true (ADR-024 §13).
        _botProtectionVerifier.ShouldSucceed = true;

        var result = await CreateGuard().VerifyAsync(ValidRequest(), "203.0.113.1");

        Assert.True(result.IsSuccess);
        Assert.Single(_botProtectionVerifier.VerifiedTokens);
    }

    [Fact]
    public async Task VerifyAsync_TokenMissingOrExpired_ReturnsRejectedWithoutCallingTurnstile()
    {
        _submissionTokenGenerator.NextValidateTokenResult = null;
        SeedSiteSettings(botProtectionEnabled: true);

        var result = await CreateGuard().VerifyAsync(ValidRequest(), "203.0.113.1");

        Assert.True(result.IsFailure);
        Assert.Equal("PublicSubmission.Rejected", result.Error.Code);
        Assert.Empty(_botProtectionVerifier.VerifiedTokens);
    }

    [Fact]
    public async Task VerifyAsync_TokenTamperedOrInvalid_ReturnsRejected()
    {
        _submissionTokenGenerator.NextValidateTokenResult = Result.Failure<DateTimeOffset>(
            Error.Validation("PublicSubmission.InvalidToken", "Invalid or expired submission token."));
        SeedSiteSettings(botProtectionEnabled: false);

        var result = await CreateGuard().VerifyAsync(ValidRequest(), "203.0.113.1");

        Assert.True(result.IsFailure);
        Assert.Equal("PublicSubmission.Rejected", result.Error.Code);
    }

    [Fact]
    public async Task VerifyAsync_SubmittedTooQuicklyAfterTokenIssued_ReturnsRejected()
    {
        _submissionTokenGenerator.NextValidateTokenResult = Result.Success(Now.AddSeconds(-1));
        SeedSiteSettings(botProtectionEnabled: false);

        var result = await CreateGuard().VerifyAsync(ValidRequest(), "203.0.113.1");

        Assert.True(result.IsFailure);
        Assert.Equal("PublicSubmission.Rejected", result.Error.Code);
        Assert.Empty(_botProtectionVerifier.VerifiedTokens);
    }

    [Fact]
    public async Task VerifyAsync_SubmittedExactlyAtMinimumFillDuration_ReturnsSuccess()
    {
        _submissionTokenGenerator.NextValidateTokenResult = Result.Success(Now - PublicSubmissionGuard.MinimumFillDuration);
        SeedSiteSettings(botProtectionEnabled: false);

        var result = await CreateGuard().VerifyAsync(ValidRequest(), "203.0.113.1");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task VerifyAsync_HoneypotFieldFilledIn_ReturnsRejectedWithoutCallingTurnstile()
    {
        _submissionTokenGenerator.NextValidateTokenResult = Result.Success(Now.AddSeconds(-5));
        SeedSiteSettings(botProtectionEnabled: true);

        var result = await CreateGuard().VerifyAsync(
            new PublicSubmissionGuardRequest("submission-token", "turnstile-token", Website: "http://spam.example"), "203.0.113.1");

        Assert.True(result.IsFailure);
        Assert.Equal("PublicSubmission.Rejected", result.Error.Code);
        Assert.Empty(_botProtectionVerifier.VerifiedTokens);
    }

    [Fact]
    public async Task VerifyAsync_BotProtectionEnabledAndTurnstileRejects_ReturnsRejected()
    {
        _submissionTokenGenerator.NextValidateTokenResult = Result.Success(Now.AddSeconds(-5));
        SeedSiteSettings(botProtectionEnabled: true);
        _botProtectionVerifier.ShouldSucceed = false;

        var result = await CreateGuard().VerifyAsync(ValidRequest(), "203.0.113.1");

        Assert.True(result.IsFailure);
        Assert.Equal("PublicSubmission.Rejected", result.Error.Code);
    }
}
