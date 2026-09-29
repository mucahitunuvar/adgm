using System.Net;
using GenclikMerkezi.Api;
using Microsoft.AspNetCore.HttpOverrides;

namespace GenclikMerkezi.IntegrationTests.Host;

public class ReverseProxyForwardedHeadersOptionsFactoryTests
{
    [Fact]
    public void Create_OnlyForwardsXForwardedFor()
    {
        var settings = new ReverseProxySettings { Enabled = true, KnownProxies = ["10.0.0.1"] };

        var options = ReverseProxyForwardedHeadersOptionsFactory.Create(settings);

        Assert.Equal(ForwardedHeaders.XForwardedFor, options.ForwardedHeaders);
    }

    [Fact]
    public void Create_MapsKnownProxiesToParsedIPAddresses()
    {
        var settings = new ReverseProxySettings { Enabled = true, KnownProxies = ["10.0.0.1", "10.0.0.2"] };

        var options = ReverseProxyForwardedHeadersOptionsFactory.Create(settings);

        Assert.Equal([IPAddress.Parse("10.0.0.1"), IPAddress.Parse("10.0.0.2")], options.KnownProxies);
    }

    [Fact]
    public void Create_MapsKnownNetworksToParsedIPNetworks()
    {
        var settings = new ReverseProxySettings { Enabled = true, KnownNetworks = ["10.0.0.0/8"] };

        var options = ReverseProxyForwardedHeadersOptionsFactory.Create(settings);

        var network = Assert.Single(options.KnownIPNetworks);
        Assert.Equal(IPAddress.Parse("10.0.0.0"), network.BaseAddress);
        Assert.Equal(8, network.PrefixLength);
    }

    [Fact]
    public void Create_WithNoKnownProxiesOrNetworks_ProducesEmptyTrustLists()
    {
        var settings = new ReverseProxySettings();

        var options = ReverseProxyForwardedHeadersOptionsFactory.Create(settings);

        Assert.Empty(options.KnownProxies);
        Assert.Empty(options.KnownIPNetworks);
    }
}
