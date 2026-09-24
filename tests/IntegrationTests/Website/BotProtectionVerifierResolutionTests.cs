using GenclikMerkezi.Contracts.Website;
using GenclikMerkezi.IntegrationTests.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §1/§12.3 Görev 8: no Website feature calls IBotProtectionVerifier yet (anonymous
// form/event-registration endpoints arrive in later Faz'lar), so nothing else in this suite
// exercises the DI wiring across Website's port and the Host's registration. Same reasoning as
// WebsiteEmailSenderTests for IWebsiteEmailSender - resolves the port the way a future handler
// would via constructor injection, against the test-swapped FakeBotProtectionVerifier (never the
// real Cloudflare-calling adapter, which is covered separately by
// CloudflareTurnstileBotProtectionVerifierTests).
public class BotProtectionVerifierResolutionTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public BotProtectionVerifierResolutionTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task VerifyAsync_ResolvedFromContainer_DelegatesToRegisteredVerifier()
    {
        _factory.BotProtectionVerifier.ShouldSucceed = true;

        using var scope = _factory.Services.CreateScope();
        var botProtectionVerifier = scope.ServiceProvider.GetRequiredService<IBotProtectionVerifier>();

        var result = await botProtectionVerifier.VerifyAsync("token-abc", "203.0.113.1");

        Assert.True(result.IsSuccess);
        Assert.Contains(_factory.BotProtectionVerifier.VerifiedTokens, t => t.Token == "token-abc" && t.RemoteIpAddress == "203.0.113.1");
    }

    [Fact]
    public async Task VerifyAsync_WhenConfiguredToFail_ReturnsFailure()
    {
        _factory.BotProtectionVerifier.ShouldSucceed = false;

        using var scope = _factory.Services.CreateScope();
        var botProtectionVerifier = scope.ServiceProvider.GetRequiredService<IBotProtectionVerifier>();

        var result = await botProtectionVerifier.VerifyAsync("token-abc", "203.0.113.1");

        Assert.True(result.IsFailure);
        Assert.Equal("BotProtection.VerificationFailed", result.Error.Code);

        _factory.BotProtectionVerifier.ShouldSucceed = true;
    }
}
