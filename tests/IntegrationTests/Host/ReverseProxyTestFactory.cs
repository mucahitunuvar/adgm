using GenclikMerkezi.IntegrationTests.Identity;
using Microsoft.AspNetCore.Hosting;

namespace GenclikMerkezi.IntegrationTests.Host;

// ADR-024 Faz 1b Görev 1: overrides just the ReverseProxy/RateLimiting settings a test needs -
// everything else (Identity/Notification/etc. LocalDB wiring, Website's Sqlite database) stays
// exactly as CustomWebApplicationFactory sets it up.
public sealed class ReverseProxyTestFactory(
    bool reverseProxyEnabled, string[] knownNetworks, int? publicReadPermitLimit = null, int? publicFormsPermitLimit = null,
    int? publicSearchPermitLimit = null)
    : CustomWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseSetting("ReverseProxy:Enabled", reverseProxyEnabled.ToString());
        for (var i = 0; i < knownNetworks.Length; i++)
        {
            builder.UseSetting($"ReverseProxy:KnownNetworks:{i}", knownNetworks[i]);
        }

        if (publicReadPermitLimit is not null)
        {
            builder.UseSetting("RateLimiting:PublicReadPermitLimit", publicReadPermitLimit.Value.ToString());
        }

        if (publicFormsPermitLimit is not null)
        {
            builder.UseSetting("RateLimiting:PublicFormsPermitLimit", publicFormsPermitLimit.Value.ToString());
        }

        // ADR-024 §10 (Faz 5 Görev 3): the public-search policy's own dedicated override, same pattern.
        if (publicSearchPermitLimit is not null)
        {
            builder.UseSetting("RateLimiting:PublicSearchPermitLimit", publicSearchPermitLimit.Value.ToString());
        }
    }
}
