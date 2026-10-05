using System.Net;
using System.Net.Http.Json;

namespace GenclikMerkezi.IntegrationTests.Host;

// ADR-024 §12.3 (Faz 3 Görev 1): end-to-end proof that GET /api/v1/public/submission-tokens is bound
// to the "public-forms" rate limit policy, separate from (and much tighter than) "public-read" -
// same approach as PublicReadRateLimitAndForwardedHeadersTests for that other policy.
public class PublicFormsRateLimitTests
{
    private const string SubmissionTokenEndpoint = "/api/v1/public/submission-tokens";

    [Fact]
    public async Task SubmissionTokenEndpoint_ExceedsConfiguredLimit_Returns429()
    {
        using var factory = new ReverseProxyTestFactory(reverseProxyEnabled: false, knownNetworks: [], publicFormsPermitLimit: 2);
        var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(SubmissionTokenEndpoint)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(SubmissionTokenEndpoint)).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await client.GetAsync(SubmissionTokenEndpoint)).StatusCode);
    }

    [Fact]
    public async Task SubmissionTokenEndpoint_WithinLimit_ReturnsAFreshTokenEachTime()
    {
        using var factory = new ReverseProxyTestFactory(reverseProxyEnabled: false, knownNetworks: [], publicFormsPermitLimit: 10);
        var client = factory.CreateClient();

        var first = await client.GetFromJsonAsync<SubmissionTokenResponse>(SubmissionTokenEndpoint);
        var second = await client.GetFromJsonAsync<SubmissionTokenResponse>(SubmissionTokenEndpoint);

        Assert.False(string.IsNullOrWhiteSpace(first!.Token));
        Assert.NotEqual(first.Token, second!.Token);
    }

    private sealed record SubmissionTokenResponse(string Token);
}
