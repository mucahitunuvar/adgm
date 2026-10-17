using System.Net;

namespace GenclikMerkezi.IntegrationTests.Host;

// GÖREV 2: /api/v1/reference-data/* (LookupEndpoints.MapReadOnly) used to be AllowAnonymous with no
// rate limiting policy at all - a previously-existing inconsistency with every other anonymous public
// GET family (Website/Employer's /api/v1/public/* endpoints, and ReferenceData's own
// /api/v1/public/reference-data/* family in PublicLookupEndpoints), which all sit behind "public-read".
// Same approach as PublicReadRateLimitAndForwardedHeadersTests for that policy, plus the "gövdesiz"
// (bodyless) 429 shape every policy in Program.cs produces: RejectionStatusCode is set with no
// OnRejected delegate, so the ASP.NET Core rate limiter middleware writes the status code alone.
public class ReferenceDataPublicReadRateLimitTests
{
    private const string ReferenceDataEndpoint = "/api/v1/reference-data/countries";

    // GetDistrictsEndpoint is a standalone endpoint (not built on LookupEndpoints.MapReadOnly like
    // every other lookup type), so it had fallen through without a "public-read" policy attached -
    // the same previously-existing inconsistency this file's other tests cover, just missed for this
    // one route. Response shape is unchanged (still PagedResult<LookupItemSummary> on success).
    [Fact]
    public async Task GetDistrictsEndpoint_ExceedsConfiguredLimit_Returns429WithEmptyBody()
    {
        using var factory = new ReverseProxyTestFactory(reverseProxyEnabled: false, knownNetworks: [], publicReadPermitLimit: 2);
        var client = factory.CreateClient();
        const string districtsEndpoint = "/api/v1/reference-data/districts";

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(districtsEndpoint)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(districtsEndpoint)).StatusCode);

        var rejected = await client.GetAsync(districtsEndpoint);

        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        var body = await rejected.Content.ReadAsStringAsync();
        Assert.Equal(string.Empty, body);
    }

    [Fact]
    public async Task ReferenceDataLookupEndpoint_ExceedsConfiguredLimit_Returns429WithEmptyBody()
    {
        using var factory = new ReverseProxyTestFactory(reverseProxyEnabled: false, knownNetworks: [], publicReadPermitLimit: 2);
        var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(ReferenceDataEndpoint)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(ReferenceDataEndpoint)).StatusCode);

        var rejected = await client.GetAsync(ReferenceDataEndpoint);

        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        var body = await rejected.Content.ReadAsStringAsync();
        Assert.Equal(string.Empty, body);
    }

    // Proves /api/v1/reference-data/* and /api/v1/public/site now share the SAME policy/quota (one
    // partition per client IP) rather than each endpoint family getting its own independent budget -
    // the second call below is already the family's 3rd request overall.
    [Fact]
    public async Task ReferenceDataLookupEndpoint_SharesPublicReadQuotaWithOtherPublicReadEndpoints()
    {
        using var factory = new ReverseProxyTestFactory(reverseProxyEnabled: false, knownNetworks: [], publicReadPermitLimit: 2);
        var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(ReferenceDataEndpoint)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/public/site")).StatusCode);

        var rejected = await client.GetAsync(ReferenceDataEndpoint);

        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
    }
}
