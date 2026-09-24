using System.Net;
using System.Text;
using GenclikMerkezi.Api.Website;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.IntegrationTests.Website;

// Exercises the real Cloudflare-calling adapter directly (no ASP.NET host, no DI) against a stub
// HttpMessageHandler, so its request/response handling is proven without ever reaching the real
// Cloudflare API. Lives here (not in UnitTests) only because GenclikMerkezi.Api - the adapter's
// project - is not referenced by GenclikMerkezi.UnitTests, matching how every other Host-only class
// (e.g. CareerAdvisorDeactivationOrchestrator) is verified from this project instead.
public class CloudflareTurnstileBotProtectionVerifierTests
{
    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(respond(request));
        }
    }

    private static HttpResponseMessage JsonResponse(string json) =>
        new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private static (CloudflareTurnstileBotProtectionVerifier Verifier, StubHttpMessageHandler Handler) CreateVerifier(
        Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        var handler = new StubHttpMessageHandler(respond);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://challenges.cloudflare.com/") };
        var options = Options.Create(new TurnstileSettings { SecretKey = "test-secret" });
        return (new CloudflareTurnstileBotProtectionVerifier(httpClient, options), handler);
    }

    [Fact]
    public async Task VerifyAsync_WithSuccessfulSiteverifyResponse_ReturnsSuccess()
    {
        var (verifier, _) = CreateVerifier(_ => JsonResponse("""{"success":true}"""));

        var result = await verifier.VerifyAsync("valid-token", "203.0.113.1", CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task VerifyAsync_WithUnsuccessfulSiteverifyResponse_ReturnsFailureIncludingErrorCodes()
    {
        var (verifier, _) = CreateVerifier(_ => JsonResponse("""{"success":false,"error-codes":["invalid-input-response"]}"""));

        var result = await verifier.VerifyAsync("bad-token", "203.0.113.1", CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("BotProtection.VerificationFailed", result.Error.Code);
        Assert.Contains("invalid-input-response", result.Error.Message);
    }

    [Fact]
    public async Task VerifyAsync_WithMissingToken_FailsWithoutCallingCloudflare()
    {
        var (verifier, handler) = CreateVerifier(_ => JsonResponse("""{"success":true}"""));

        var result = await verifier.VerifyAsync(null, "203.0.113.1", CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("BotProtection.TokenMissing", result.Error.Code);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task VerifyAsync_WhenCloudflareIsUnreachable_FailsClosed()
    {
        var (verifier, _) = CreateVerifier(_ => throw new HttpRequestException("Connection refused"));

        var result = await verifier.VerifyAsync("valid-token", "203.0.113.1", CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("BotProtection.VerificationUnavailable", result.Error.Code);
    }

    [Fact]
    public async Task VerifyAsync_WithMalformedJsonResponse_FailsClosed()
    {
        var (verifier, _) = CreateVerifier(_ => JsonResponse("not json"));

        var result = await verifier.VerifyAsync("valid-token", "203.0.113.1", CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("BotProtection.VerificationUnavailable", result.Error.Code);
    }

    [Fact]
    public async Task VerifyAsync_SendsSecretResponseAndRemoteIpAsFormFields()
    {
        HttpRequestMessage? capturedRequest = null;
        string? capturedBody = null;
        var (verifier, _) = CreateVerifier(request =>
        {
            capturedRequest = request;
            capturedBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonResponse("""{"success":true}""");
        });

        await verifier.VerifyAsync("the-token", "203.0.113.1", CancellationToken.None);

        Assert.Equal("turnstile/v0/siteverify", capturedRequest!.RequestUri!.AbsolutePath.TrimStart('/'));
        Assert.Contains("secret=test-secret", capturedBody);
        Assert.Contains("response=the-token", capturedBody);
        Assert.Contains("remoteip=203.0.113.1", capturedBody);
    }
}
