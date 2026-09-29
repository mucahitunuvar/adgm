using System.Net;
using GenclikMerkezi.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;

namespace GenclikMerkezi.IntegrationTests.Host;

// ADR-024 Faz 1b Görev 1: verifies ForwardedHeadersMiddleware trust behavior (built from
// ReverseProxyForwardedHeadersOptionsFactory - the same factory Program.cs uses) directly against a
// minimal pipeline with a simulated connecting address, rather than through the full
// CustomWebApplicationFactory app. Microsoft.AspNetCore.TestHost's simulated connection never has a
// real RemoteIpAddress (it is null by default), so ForwardedHeadersMiddleware - which only enforces
// the known-proxy/network trust check against a non-null starting address - would otherwise always
// honor X-Forwarded-For regardless of KnownProxies/KnownNetworks, making the "untrusted proxy is
// ignored" half of this behavior untestable through the real app. A middleware placed before
// UseForwardedHeaders sets a specific fake RemoteIpAddress instead, simulating what a real reverse
// proxy's own connecting address would be.
public class ForwardedHeadersTrustBehaviorTests
{
    private static async Task<string> ResolveRemoteIpAsync(
        ReverseProxySettings settings, IPAddress simulatedConnectingAddress, string forwardedFor)
    {
        var hostBuilder = new WebHostBuilder()
            .UseTestServer()
            .Configure(app =>
            {
                app.Use(async (context, next) =>
                {
                    context.Connection.RemoteIpAddress = simulatedConnectingAddress;
                    await next();
                });
                app.UseForwardedHeaders(ReverseProxyForwardedHeadersOptionsFactory.Create(settings));
                app.Run(context => context.Response.WriteAsync(context.Connection.RemoteIpAddress?.ToString() ?? "NULL"));
            });

        using var testServer = new TestServer(hostBuilder);
        var client = testServer.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("X-Forwarded-For", forwardedFor);
        var response = await client.SendAsync(request);

        return await response.Content.ReadAsStringAsync();
    }

    [Fact]
    public async Task ConnectingAddressWithinKnownNetwork_XForwardedForIsHonored()
    {
        var settings = new ReverseProxySettings { Enabled = true, KnownNetworks = ["10.0.0.0/8"] };

        var resolvedIp = await ResolveRemoteIpAsync(settings, IPAddress.Parse("10.0.0.5"), "203.0.113.9");

        Assert.Equal("203.0.113.9", resolvedIp);
    }

    [Fact]
    public async Task ConnectingAddressOutsideKnownNetwork_XForwardedForIsIgnored()
    {
        var settings = new ReverseProxySettings { Enabled = true, KnownNetworks = ["10.0.0.0/8"] };

        var resolvedIp = await ResolveRemoteIpAsync(settings, IPAddress.Parse("203.0.113.1"), "203.0.113.9");

        Assert.Equal("203.0.113.1", resolvedIp);
    }
}
