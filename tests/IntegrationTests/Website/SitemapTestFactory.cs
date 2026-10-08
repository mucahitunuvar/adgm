using GenclikMerkezi.IntegrationTests.Identity;
using Microsoft.AspNetCore.Hosting;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §15 (Faz 5 Görev 5): overrides just the sitemap-related settings a test needs - everything
// else stays exactly as CustomWebApplicationFactory sets it up, same shape as ReverseProxyTestFactory.
// xUnit's IClassFixture<T> support requires a fixture type to have EXACTLY one public constructor, so
// the configurable (publicSiteBaseUrl, maxUrlsPerSitemapFile) constructor below is internal - still
// reachable from SitemapAndRobotsFlowTests (same assembly) via `new SitemapTestFactory(...)` outside
// IClassFixture, the same direct-construction pattern ReverseProxyTestFactory's own callers use.
public sealed class SitemapTestFactory : CustomWebApplicationFactory
{
    private readonly string _publicSiteBaseUrl;
    private readonly int? _maxUrlsPerSitemapFile;

    public SitemapTestFactory()
        : this("https://example.org", null)
    {
    }

    internal SitemapTestFactory(string publicSiteBaseUrl, int? maxUrlsPerSitemapFile)
    {
        _publicSiteBaseUrl = publicSiteBaseUrl;
        _maxUrlsPerSitemapFile = maxUrlsPerSitemapFile;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseSetting("Website:PublicSiteBaseUrl", _publicSiteBaseUrl);

        if (_maxUrlsPerSitemapFile is not null)
        {
            builder.UseSetting("Website:Sitemap:MaxUrlsPerSitemapFile", _maxUrlsPerSitemapFile.Value.ToString());
        }
    }
}
