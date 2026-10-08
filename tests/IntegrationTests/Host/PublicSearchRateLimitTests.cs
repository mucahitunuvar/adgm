using System.Net;

namespace GenclikMerkezi.IntegrationTests.Host;

// ADR-024 §10 (Faz 5 Görev 3). The rate limiter runs ahead of MediatR/the handler, so this does not
// need GlobalSearchEnabled turned on or any seeded SearchDocument - every request below counts toward
// the "public-search" policy's quota (and gets a 404, since the flag is off by default) until the
// third one is rejected with 429, exactly like PublicReadRateLimitAndForwardedHeadersTests proves for
// "public-read".
public class PublicSearchRateLimitTests
{
    private const string PublicSearchEndpoint = "/api/v1/public/search?q=kariyer";

    [Fact]
    public async Task PublicSearchEndpoint_ExceedsConfiguredLimit_Returns429()
    {
        using var factory = new ReverseProxyTestFactory(reverseProxyEnabled: false, knownNetworks: [], publicSearchPermitLimit: 2);
        var client = factory.CreateClient();

        Assert.NotEqual(HttpStatusCode.TooManyRequests, (await client.GetAsync(PublicSearchEndpoint)).StatusCode);
        Assert.NotEqual(HttpStatusCode.TooManyRequests, (await client.GetAsync(PublicSearchEndpoint)).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await client.GetAsync(PublicSearchEndpoint)).StatusCode);
    }
}
