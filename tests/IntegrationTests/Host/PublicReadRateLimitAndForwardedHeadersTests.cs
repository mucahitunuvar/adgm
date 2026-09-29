using System.Net;

namespace GenclikMerkezi.IntegrationTests.Host;

// ADR-024 Faz 1b Görev 1. End-to-end tests against the real app (via ReverseProxyTestFactory) for the
// public-read rate limit policy and for ReverseProxy's fail-fast startup validation. The "known proxy
// honored / unknown proxy ignored" trust-check behavior itself is covered separately in
// ForwardedHeadersTrustBehaviorTests, which controls the simulated connecting address directly -
// TestServer's default (null) RemoteIpAddress makes that distinction untestable through the full app
// (see that file's header comment).
public class PublicReadRateLimitAndForwardedHeadersTests
{
    private const string PublicReadEndpoint = "/api/v1/public/site";

    private static Task<HttpResponseMessage> GetWithForwardedForAsync(HttpClient client, string forwardedIp)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, PublicReadEndpoint);
        request.Headers.Add("X-Forwarded-For", forwardedIp);
        return client.SendAsync(request);
    }

    [Fact]
    public async Task PublicReadEndpoint_ExceedsConfiguredLimit_Returns429()
    {
        using var factory = new ReverseProxyTestFactory(reverseProxyEnabled: false, knownNetworks: [], publicReadPermitLimit: 2);
        var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(PublicReadEndpoint)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(PublicReadEndpoint)).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await client.GetAsync(PublicReadEndpoint)).StatusCode);
    }

    [Fact]
    public void Factory_WhenReverseProxyEnabledWithEmptyKnownProxiesAndNetworks_FailsToStart()
    {
        using var factory = new ReverseProxyTestFactory(reverseProxyEnabled: true, knownNetworks: []);

        Assert.ThrowsAny<Exception>(() => factory.CreateClient());
    }

    // KnownNetworks = 0.0.0.0/0 trusts every IPv4 address as a forwarding proxy - regardless of what
    // TestServer's own simulated connecting address actually is, it is always "known", so
    // X-Forwarded-For must be honored. This proves the whole real pipeline (config -> options ->
    // ForwardedHeadersMiddleware -> rate limiter partition key) is wired correctly end-to-end.
    [Fact]
    public async Task KnownProxyNetwork_XForwardedForIsHonored_EachForgedIpGetsItsOwnQuota()
    {
        using var factory = new ReverseProxyTestFactory(reverseProxyEnabled: true, knownNetworks: ["0.0.0.0/0"], publicReadPermitLimit: 2);
        var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await GetWithForwardedForAsync(client, "9.9.9.1")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await GetWithForwardedForAsync(client, "9.9.9.1")).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await GetWithForwardedForAsync(client, "9.9.9.1")).StatusCode);

        // A different forged IP gets a fresh quota - only possible if the header was actually used to
        // partition the rate limiter, not the real (shared) TestServer connection.
        Assert.Equal(HttpStatusCode.OK, (await GetWithForwardedForAsync(client, "9.9.9.2")).StatusCode);
    }
}
