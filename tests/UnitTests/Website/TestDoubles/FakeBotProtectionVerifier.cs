using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeBotProtectionVerifier : IBotProtectionVerifier
{
    private readonly List<(string? Token, string? RemoteIpAddress)> _verifiedTokens = [];

    public bool ShouldSucceed { get; set; } = true;

    public IReadOnlyList<(string? Token, string? RemoteIpAddress)> VerifiedTokens => _verifiedTokens.AsReadOnly();

    public Task<Result> VerifyAsync(string? token, string? remoteIpAddress, CancellationToken cancellationToken = default)
    {
        _verifiedTokens.Add((token, remoteIpAddress));

        return Task.FromResult(ShouldSucceed
            ? Result.Success()
            : Result.Failure(Error.Validation("BotProtection.VerificationFailed", "Bot protection challenge failed.")));
    }
}
