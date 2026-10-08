using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Xml.Linq;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.GetSiteSettings;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsFeatures;
using GenclikMerkezi.Modules.Website.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §15 (Faz 5 Görev 5): the 10.000+ URL sitemap-index-splitting scenario, in its own test class
// with its own per-test factory (`using var factory = new SitemapTestFactory(...)`, the same
// direct-construction pattern ReverseProxyTestFactory's own callers use) rather than sharing
// SitemapAndRobotsFlowTests' IClassFixture<SitemapTestFactory> instance. Mixing an IClassFixture-shared
// factory with an ad-hoc per-test one in the same class corrupts Hangfire's process-wide
// GlobalJobFilters/JobFilterProviders statics once the ad-hoc factory's host is disposed mid-class
// (ObjectDisposedException on every later request against the SHARED factory in that same class) -
// a pre-existing sharp edge of testing multiple WebApplicationFactory hosts side by side, not something
// specific to the sitemap feature itself. Isolating this scenario into its own class, with no sibling
// shared fixture alive at the same time, avoids it entirely.
public class SitemapSplittingFlowTests
{
    private const string BaseUrl = "https://example.org";
    private static readonly XNamespace SitemapNs = "http://www.sitemaps.org/schemas/sitemap/0.9";

    [Fact]
    public async Task Sitemap_SplitsIntoIndexAndSegments_WhenEntryCountExceedsConfiguredMax()
    {
        using var factory = new SitemapTestFactory(BaseUrl, maxUrlsPerSitemapFile: 2);
        using var client = factory.CreateClient();
        var email = $"admin-{Guid.NewGuid():N}@example.com";
        await factory.SeedAdminUserAsync(email, "AdminSifre123");
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "AdminSifre123" });
        var accessToken = (await loginResponse.Content.ReadFromJsonAsync<LoginResponse>())!.AccessToken;

        HttpRequestMessage Auth(HttpMethod method, string path, object? body = null)
        {
            var request = new HttpRequestMessage(method, path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            if (body is not null)
            {
                request.Content = JsonContent.Create(body);
            }

            return request;
        }

        var settingsResponse = await client.SendAsync(Auth(HttpMethod.Get, "/api/v1/admin/website/settings"));
        var settings = await settingsResponse.Content.ReadFromJsonAsync<SiteSettingsResponse>();
        var featuresResponse = await client.SendAsync(Auth(
            HttpMethod.Put, "/api/v1/admin/website/settings/features",
            new UpdateSiteSettingsFeaturesRequest(
                settings!.RowVersion, settings.GlobalSearchEnabled, settings.NewsletterEnabled, settings.PublicJobListingsEnabled,
                settings.DonationPageEnabled, true)));
        Assert.Equal(HttpStatusCode.NoContent, featuresResponse.StatusCode);

        // Home ("/") plus 3 external job documents comfortably exceeds the configured max of 2 per file.
        for (var i = 0; i < 3; i++)
        {
            var jobId = Guid.NewGuid().ToString("N");
            using var scope = factory.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ISearchDocumentRepository>();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebsiteDbContext>();
            var document = SearchDocument.Create(
                "employer.job", jobId, LanguageCode.Create("tr").Value, "job", "İş İlanı", "Özet", $"/ilanlar/{jobId}", "IS ILANI",
                DateTime.UtcNow, DateTime.UtcNow, includeInSitemap: true).Value;
            await repository.UpsertAsync(document);
            await dbContext.SaveChangesAsync();
        }

        var rootResponse = await client.GetAsync("/sitemap.xml");
        var rootXml = XDocument.Parse(await rootResponse.Content.ReadAsStringAsync());
        var sitemapLocs = rootXml.Root!.Elements(SitemapNs + "sitemap").Select(s => s.Element(SitemapNs + "loc")!.Value).ToList();

        Assert.True(sitemapLocs.Count >= 2, "Expected the root sitemap to become a sitemap index with at least 2 segments.");
        Assert.All(sitemapLocs, loc => Assert.Matches(@"^https://example\.org/sitemap-\d+\.xml$", loc));

        var allSegmentLocs = new List<string>();
        foreach (var segmentLoc in sitemapLocs)
        {
            var segmentPath = segmentLoc.Replace(BaseUrl, string.Empty);
            var segmentResponse = await client.GetAsync(segmentPath);
            Assert.Equal(HttpStatusCode.OK, segmentResponse.StatusCode);
            var segmentXml = XDocument.Parse(await segmentResponse.Content.ReadAsStringAsync());
            allSegmentLocs.AddRange(segmentXml.Root!.Elements(SitemapNs + "url").Select(u => u.Element(SitemapNs + "loc")!.Value));
        }

        // Not an exact count (the seeded content-type catalog's own listing-page count is incidental to
        // this test) - just that every expected entry landed in exactly one segment, with no loss or
        // duplication across the split.
        Assert.Equal(allSegmentLocs.Count, allSegmentLocs.Distinct().Count());
        Assert.Contains($"{BaseUrl}/", allSegmentLocs);
        Assert.True(allSegmentLocs.Count >= 4, "Expected at least the home page plus the 3 seeded job documents.");

        var outOfRangeResponse = await client.GetAsync("/sitemap-99.xml");
        Assert.Equal(HttpStatusCode.NotFound, outOfRangeResponse.StatusCode);
    }
}
