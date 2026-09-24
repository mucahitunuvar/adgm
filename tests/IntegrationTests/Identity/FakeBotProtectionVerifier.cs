using GenclikMerkezi.Contracts.Website;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.IntegrationTests.Identity;

// Replaces the real CloudflareTurnstileBotProtectionVerifier for integration tests (no real
// Cloudflare call involved). Succeeds by default - toggle ShouldSucceed to exercise a rejected
// submission once a Faz 3/4 anonymous endpoint actually calls this port.
public sealed class FakeBotProtectionVerifier : IBotProtectionVerifier
{
    private readonly List<(string? Token, string? RemoteIpAddress)> _verifiedTokens = [];

    public bool ShouldSucceed { get; set; } = true;

    public IReadOnlyCollection<(string? Token, string? RemoteIpAddress)> VerifiedTokens => _verifiedTokens.AsReadOnly();

    public Task<Result> VerifyAsync(string? token, string? remoteIpAddress, CancellationToken cancellationToken = default)
    {
        _verifiedTokens.Add((token, remoteIpAddress));

        return Task.FromResult(ShouldSucceed
            ? Result.Success()
            : Result.Failure(Error.Validation("BotProtection.VerificationFailed", "Bot protection challenge failed.")));
    }
}
