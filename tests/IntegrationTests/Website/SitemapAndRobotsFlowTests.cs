using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Xml.Linq;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.ArchiveContentItem;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.GetContentItemById;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.GetSiteSettings;
using GenclikMerkezi.Modules.Website.Features.PublishContentItem;
using GenclikMerkezi.Modules.Website.Features.UnpublishContentItem;
using GenclikMerkezi.Modules.Website.Features.UpdateContentItemTranslation;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsFeatures;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsMaintenance;
using GenclikMerkezi.Modules.Website.Infrastructure;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §15 (Faz 5 Görev 5): /sitemap.xml, /sitemap-{n}.xml and /robots.txt. SiteSettings is a
// singleton shared by every test method in this class (IClassFixture), so every test explicitly sets
// the flags it depends on rather than assuming the fixture's virgin default - the same caveat every
// other Website flow test class documents. Website:PublicSiteBaseUrl is fixed to "https://example.org"
// by SitemapTestFactory so every <loc>/href assertion below can compare full, absolute URLs.
public class SitemapAndRobotsFlowTests : IClassFixture<SitemapTestFactory>
{
    private const string BaseUrl = "https://example.org";
    private static readonly XNamespace SitemapNs = "http://www.sitemaps.org/schemas/sitemap/0.9";
    private static readonly XNamespace XhtmlNs = "http://www.w3.org/1999/xhtml";

    private readonly SitemapTestFactory _factory;
    private readonly HttpClient _client;

    public SitemapAndRobotsFlowTests(SitemapTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> LoginAsAdminAsync()
    {
        var email = $"admin-{Guid.NewGuid():N}@example.com";
        const string password = "AdminSifre123";
        await _factory.SeedAdminUserAsync(email, password);

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return login!.AccessToken;
    }

    private HttpRequestMessage Authorized(HttpMethod method, string path, string accessToken, object? body = null)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }

    private async Task<SiteSettingsResponse> GetSiteSettingsAsync(string accessToken)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/settings", accessToken));
        return (await response.Content.ReadFromJsonAsync<SiteSettingsResponse>())!;
    }

    private async Task SetAllowSearchEngineIndexingAsync(string accessToken, bool allowed)
    {
        var settings = await GetSiteSettingsAsync(accessToken);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, "/api/v1/admin/website/settings/features", accessToken,
            new UpdateSiteSettingsFeaturesRequest(
                settings.RowVersion, settings.GlobalSearchEnabled, settings.NewsletterEnabled, settings.PublicJobListingsEnabled,
                settings.DonationPageEnabled, allowed)));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task SetMaintenanceModeAsync(string accessToken, bool enabled)
    {
        var settings = await GetSiteSettingsAsync(accessToken);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, "/api/v1/admin/website/settings/maintenance", accessToken,
            new UpdateSiteSettingsMaintenanceRequest(settings.RowVersion, enabled, [])));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<Guid> GetContentTypeIdByKeyAsync(string accessToken, string key)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/content-types", accessToken));
        var types = await response.Content.ReadFromJsonAsync<List<ContentTypeSummaryResponse>>();
        return types!.Single(t => t.Key == key).Id;
    }

    private async Task<ContentItemDetailResponse> GetContentItemAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{id}", accessToken));
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"GET contents/{id} failed with {response.StatusCode}: {body}");
        return System.Text.Json.JsonSerializer.Deserialize<ContentItemDetailResponse>(
            body, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web))!;
    }

    private async Task<Guid> CreateContentItemAsync(string accessToken, Guid contentTypeId, string title, bool noIndex = false)
    {
        var seo = new CreateContentItemSeoInput(null, null, null, null, null, null, null, noIndex);
        var slug = $"slug-{Guid.NewGuid():N}";
        var request = new CreateContentItemRequest(contentTypeId, null, 1, false, null, null, title, slug, "Özet", "Gövde", seo);
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, request));
        var createBody = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"POST contents failed with {response.StatusCode}: {createBody}");
        var created = System.Text.Json.JsonSerializer.Deserialize<CreateContentItemResponse>(
            createBody, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
        return created!.Id;
    }

    private async Task PublishAsync(string accessToken, Guid id)
    {
        var rowVersion = (await GetContentItemAsync(accessToken, id)).RowVersion;
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{id}/publish", accessToken, new PublishContentItemRequest(rowVersion, null, null)));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task UnpublishAsync(string accessToken, Guid id)
    {
        var rowVersion = (await GetContentItemAsync(accessToken, id)).RowVersion;
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{id}/unpublish", accessToken, new UnpublishContentItemRequest(rowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task ArchiveAsync(string accessToken, Guid id)
    {
        var rowVersion = (await GetContentItemAsync(accessToken, id)).RowVersion;
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{id}/archive", accessToken, new ArchiveContentItemRequest(rowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<Guid> CreateAndPublishAsync(string accessToken, Guid contentTypeId, string title, bool noIndex = false)
    {
        var id = await CreateContentItemAsync(accessToken, contentTypeId, title, noIndex);
        await PublishAsync(accessToken, id);
        return id;
    }

    // Bypasses the real sync pipeline (ExternalSearchSourceSynchronizer), same shortcut
    // PublicSearchEndpointFlowTests.SeedDocumentAsync already uses for SearchDocument rows - but the
    // sitemap IS cached (unlike search results), so this must invalidate the cache itself, exactly as
    // ExternalSearchSourceSynchronizer now does after a real sync, or a sitemap request from an earlier
    // test in this shared-fixture class would still serve its stale, pre-seed snapshot.
    private async Task SeedExternalSearchDocumentAsync(string sourceKey, string sourceId, string url, bool includeInSitemap)
    {
        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchDocumentRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<WebsiteDbContext>();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

        var document = SearchDocument.Create(
            sourceKey, sourceId, LanguageCode.Create("tr").Value, "job", "İş İlanı", "Özet", url, "IS ILANI OZET",
            DateTime.UtcNow, DateTime.UtcNow, includeInSitemap).Value;
        await repository.UpsertAsync(document);
        await dbContext.SaveChangesAsync();
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);
    }

    private async Task<XDocument> GetSitemapXmlAsync(HttpClient? client = null)
    {
        var response = await (client ?? _client).GetAsync("/sitemap.xml");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("application/xml", response.Content.Headers.ContentType!.MediaType);
        return XDocument.Parse(await response.Content.ReadAsStringAsync());
    }

    private static IReadOnlyList<string> ExtractLocs(XDocument sitemap) =>
        sitemap.Root!.Elements(SitemapNs + "url").Select(u => u.Element(SitemapNs + "loc")!.Value).ToList();

    [Fact]
    public async Task Sitemap_IndexingDisabled_ReturnsEmptyUrlSet()
    {
        var accessToken = await LoginAsAdminAsync();
        await SetAllowSearchEngineIndexingAsync(accessToken, allowed: false);

        var sitemap = await GetSitemapXmlAsync();

        Assert.Empty(sitemap.Root!.Elements(SitemapNs + "url"));

        await SetAllowSearchEngineIndexingAsync(accessToken, allowed: true);
    }

    [Fact]
    public async Task Sitemap_IncludesHomeAndListingPages()
    {
        var accessToken = await LoginAsAdminAsync();
        await SetAllowSearchEngineIndexingAsync(accessToken, allowed: true);

        var sitemap = await GetSitemapXmlAsync();
        var locs = ExtractLocs(sitemap);

        Assert.Contains($"{BaseUrl}/", locs);
        Assert.Contains($"{BaseUrl}/haberler", locs);
    }

    [Fact]
    public async Task Sitemap_VisibilityTable_OnlyPublishedVisibleNonNoIndexContentIsIncluded()
    {
        var accessToken = await LoginAsAdminAsync();
        await SetAllowSearchEngineIndexingAsync(accessToken, allowed: true);
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");

        var published = await CreateAndPublishAsync(accessToken, newsTypeId, $"Published {Guid.NewGuid():N}");

        var draftId = await CreateContentItemAsync(accessToken, newsTypeId, $"Draft {Guid.NewGuid():N}");

        var unpublishedId = await CreateAndPublishAsync(accessToken, newsTypeId, $"Unpublished {Guid.NewGuid():N}");
        await UnpublishAsync(accessToken, unpublishedId);

        var archivedId = await CreateAndPublishAsync(accessToken, newsTypeId, $"Archived {Guid.NewGuid():N}");
        await UnpublishAsync(accessToken, archivedId);
        await ArchiveAsync(accessToken, archivedId);

        var noIndexId = await CreateAndPublishAsync(accessToken, newsTypeId, $"NoIndex {Guid.NewGuid():N}", noIndex: true);

        var publishedPath = (await GetContentItemAsync(accessToken, published)).Translations.Single(t => t.LanguageCode == "tr").FullPath;
        var draftPath = (await GetContentItemAsync(accessToken, draftId)).Translations.Single(t => t.LanguageCode == "tr").FullPath;
        var unpublishedPath = (await GetContentItemAsync(accessToken, unpublishedId)).Translations.Single(t => t.LanguageCode == "tr").FullPath;
        var archivedPath = (await GetContentItemAsync(accessToken, archivedId)).Translations.Single(t => t.LanguageCode == "tr").FullPath;
        var noIndexPath = (await GetContentItemAsync(accessToken, noIndexId)).Translations.Single(t => t.LanguageCode == "tr").FullPath;

        var sitemap = await GetSitemapXmlAsync();
        var locs = ExtractLocs(sitemap);

        Assert.Contains($"{BaseUrl}/{publishedPath}", locs);
        Assert.DoesNotContain($"{BaseUrl}/{draftPath}", locs);
        Assert.DoesNotContain($"{BaseUrl}/{unpublishedPath}", locs);
        Assert.DoesNotContain($"{BaseUrl}/{archivedPath}", locs);
        Assert.DoesNotContain($"{BaseUrl}/{noIndexPath}", locs);
    }

    [Fact]
    public async Task Sitemap_IncludesExternalSourceDocumentsFlaggedIncludeInSitemap_ButNotWebsiteSourceOrUnflaggedOnes()
    {
        var accessToken = await LoginAsAdminAsync();
        await SetAllowSearchEngineIndexingAsync(accessToken, allowed: true);

        var includedId = Guid.NewGuid().ToString("N");
        var excludedId = Guid.NewGuid().ToString("N");
        var websiteId = Guid.NewGuid().ToString("N");
        await SeedExternalSearchDocumentAsync("employer.job", includedId, $"/ilanlar/{includedId}", includeInSitemap: true);
        await SeedExternalSearchDocumentAsync("employer.job", excludedId, $"/ilanlar/{excludedId}", includeInSitemap: false);
        await SeedExternalSearchDocumentAsync("website", websiteId, $"/t/{websiteId}", includeInSitemap: true);

        var sitemap = await GetSitemapXmlAsync();
        var locs = ExtractLocs(sitemap);

        Assert.Contains($"{BaseUrl}/ilanlar/{includedId}", locs);
        Assert.DoesNotContain($"{BaseUrl}/ilanlar/{excludedId}", locs);
        // "website" source documents are rebuilt straight from ContentItem, never read back from
        // SearchDocument - a stray "website" row (there is none in real operation) must never surface.
        Assert.DoesNotContain($"{BaseUrl}/t/{websiteId}", locs);
    }

    [Fact]
    public async Task Sitemap_HomePage_HasHreflangForEveryActiveLanguagePlusXDefault()
    {
        var accessToken = await LoginAsAdminAsync();
        await SetAllowSearchEngineIndexingAsync(accessToken, allowed: true);

        var languagesResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/languages", accessToken));
        var languages = await languagesResponse.Content.ReadFromJsonAsync<GenclikMerkezi.Modules.Website.Features.GetSiteLanguages.GetSiteLanguagesResponse>();
        var english = languages!.Items.Single(l => l.Code == "en");
        if (!english.IsActive)
        {
            var activateResponse = await _client.SendAsync(
                Authorized(HttpMethod.Post, $"/api/v1/admin/website/languages/{english.Id}/activate", accessToken));
            Assert.Equal(HttpStatusCode.NoContent, activateResponse.StatusCode);
        }

        var sitemap = await GetSitemapXmlAsync();
        var homeUrl = sitemap.Root!.Elements(SitemapNs + "url").Single(u => u.Element(SitemapNs + "loc")!.Value == $"{BaseUrl}/");
        var alternateLinks = homeUrl.Elements(XhtmlNs + "link").ToList();

        Assert.Contains(alternateLinks, l => l.Attribute("hreflang")!.Value == "tr" && l.Attribute("href")!.Value == $"{BaseUrl}/");
        Assert.Contains(alternateLinks, l => l.Attribute("hreflang")!.Value == "en" && l.Attribute("href")!.Value == $"{BaseUrl}/en");
        Assert.Contains(alternateLinks, l => l.Attribute("hreflang")!.Value == "x-default" && l.Attribute("href")!.Value == $"{BaseUrl}/");
    }

    [Fact]
    public async Task Sitemap_WhenNotSplit_RequestingASegmentDirectly_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        await SetAllowSearchEngineIndexingAsync(accessToken, allowed: true);

        var response = await _client.GetAsync("/sitemap-1.xml");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Robots_Normal_ReturnsDisallowListAndSitemapLine()
    {
        var accessToken = await LoginAsAdminAsync();
        await SetAllowSearchEngineIndexingAsync(accessToken, allowed: true);
        await SetMaintenanceModeAsync(accessToken, enabled: false);

        var response = await _client.GetAsync("/robots.txt");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("text/plain", response.Content.Headers.ContentType!.MediaType);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains("User-agent: *", body);
        Assert.Contains("Disallow: /api/", body);
        Assert.Contains($"Sitemap: {BaseUrl}/sitemap.xml", body);
        Assert.DoesNotContain("Disallow: /\n", body);
    }

    [Fact]
    public async Task Robots_MaintenanceModeAndIndexingDisabled_DisallowsEverything()
    {
        var accessToken = await LoginAsAdminAsync();
        await SetAllowSearchEngineIndexingAsync(accessToken, allowed: false);
        await SetMaintenanceModeAsync(accessToken, enabled: true);

        var response = await _client.GetAsync("/robots.txt");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains("Disallow: /\n", body);
        Assert.DoesNotContain("Disallow: /api/", body);

        await SetAllowSearchEngineIndexingAsync(accessToken, allowed: true);
        await SetMaintenanceModeAsync(accessToken, enabled: false);
    }

    [Fact]
    public async Task Robots_MaintenanceModeAloneWithoutIndexingDisabled_StillReturnsNormalDisallowList()
    {
        var accessToken = await LoginAsAdminAsync();
        await SetAllowSearchEngineIndexingAsync(accessToken, allowed: true);
        await SetMaintenanceModeAsync(accessToken, enabled: true);

        var response = await _client.GetAsync("/robots.txt");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains("Disallow: /api/", body);
        Assert.DoesNotContain("Disallow: /\n", body);

        await SetMaintenanceModeAsync(accessToken, enabled: false);
    }
}
