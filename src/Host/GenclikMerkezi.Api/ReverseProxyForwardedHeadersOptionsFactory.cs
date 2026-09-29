using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace GenclikMerkezi.Api;

// Extracted out of Program.cs purely so a test can verify ReverseProxySettings -> ForwardedHeadersOptions
// mapping (and, combined with a minimal test pipeline, the resulting trust behavior) without needing a
// full WebApplicationFactory<Program> host - see PublicReadRateLimitAndForwardedHeadersTests.
public static class ReverseProxyForwardedHeadersOptionsFactory
{
    public static ForwardedHeadersOptions Create(ReverseProxySettings settings)
    {
        var options = new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor };

        // Defaults to loopback-only trust; this module manages the trusted set entirely from
        // ReverseProxySettings, so both start empty regardless of what the framework default is.
        options.KnownProxies.Clear();
        options.KnownIPNetworks.Clear();

        foreach (var proxy in settings.KnownProxies)
        {
            options.KnownProxies.Add(IPAddress.Parse(proxy));
        }

        foreach (var network in settings.KnownNetworks)
        {
            options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
        }

        return options;
    }
}
