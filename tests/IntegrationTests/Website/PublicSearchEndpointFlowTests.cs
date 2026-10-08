using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.GetPublicSearch;
using GenclikMerkezi.Modules.Website.Features.GetSiteSettings;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsFeatures;
using GenclikMerkezi.Modules.Website.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §10 (Faz 5 Görev 3). Documents are seeded directly through ISearchDocumentRepository (same
// no-content-pipeline shortcut SearchDocumentPersistenceFlowTests/Görev 1 uses) - Görev 2's indexing
// pipeline is exercised separately and is irrelevant to this endpoint's own matching/ranking/paging
// contract. SiteSettings is a singleton shared by every test method in this class (IClassFixture), so
// every test explicitly sets GlobalSearchEnabled rather than assuming the fixture's virgin default.
public class PublicSearchEndpointFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PublicSearchEndpointFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static LanguageCode Tr() => LanguageCode.Create("tr").Value;

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

    private async Task SetGlobalSearchEnabledAsync(string accessToken, bool enabled)
    {
        var settings = await GetSiteSettingsAsync(accessToken);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, "/api/v1/admin/website/settings/features", accessToken,
            new UpdateSiteSettingsFeaturesRequest(
                settings.RowVersion, enabled, settings.NewsletterEnabled, settings.PublicJobListingsEnabled, settings.DonationPageEnabled)));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task SeedDocumentAsync(
        string sourceKey, string sourceId, string typeKey, string title, string summary, string normalizedText,
        DateTime publishedAtUtc, LanguageCode? languageCode = null)
    {
        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchDocumentRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<WebsiteDbContext>();

        var document = SearchDocument.Create(
            sourceKey, sourceId, languageCode ?? Tr(), typeKey, title, summary, $"/t/{sourceId}", normalizedText,
            publishedAtUtc, publishedAtUtc, includeInSitemap: true).Value;
        await repository.UpsertAsync(document);
        await dbContext.SaveChangesAsync();
    }

    private async Task<PublicSearchResponse> SearchAsync(string query)
    {
        var response = await _client.GetAsync($"/api/v1/public/search?q={Uri.EscapeDataString(query)}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<PublicSearchResponse>())!;
    }

    [Fact]
    public async Task Search_WhenGlobalSearchDisabled_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        await SetGlobalSearchEnabledAsync(accessToken, enabled: false);

        var response = await _client.GetAsync("/api/v1/public/search?q=kariyer");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Search_WithInactiveLanguage_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        await SetGlobalSearchEnabledAsync(accessToken, enabled: true);

        var response = await _client.GetAsync("/api/v1/public/search?q=kariyer&lang=xx");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("")]
    public async Task Search_WithTooShortQuery_ReturnsBadRequest(string query)
    {
        var accessToken = await LoginAsAdminAsync();
        await SetGlobalSearchEnabledAsync(accessToken, enabled: true);

        var response = await _client.GetAsync($"/api/v1/public/search?q={query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_WithTooLongQuery_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        await SetGlobalSearchEnabledAsync(accessToken, enabled: true);

        var response = await _client.GetAsync($"/api/v1/public/search?q={new string('a', 101)}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_RequiresEveryToken_MissingOneTokenExcludesTheDocument()
    {
        var accessToken = await LoginAsAdminAsync();
        await SetGlobalSearchEnabledAsync(accessToken, enabled: true);

        var matchingId = Guid.NewGuid().ToString("N");
        var partialId = Guid.NewGuid().ToString("N");
        await SeedDocumentAsync(
            "website", matchingId, "news", "Kariyer Merkezi Açıldı", "Özet", "KARIYER MERKEZI ACILDI OZET", DateTime.UtcNow);
        await SeedDocumentAsync(
            "website", partialId, "news", "Kariyer Haberi", "Özet", "KARIYER HABERI OZET", DateTime.UtcNow);

        var result = await SearchAsync("kariyer merkezi");

        var urls = result.Results.Items.Select(i => i.Url).ToList();
        Assert.Contains($"/t/{matchingId}", urls);
        Assert.DoesNotContain($"/t/{partialId}", urls);
    }

    [Theory]
    [InlineData("İSTANBUL")]
    [InlineData("istanbul")]
    [InlineData("ıstanbul")]
    public async Task Search_IsTurkishCaseAndDiacriticInsensitive(string query)
    {
        var accessToken = await LoginAsAdminAsync();
        await SetGlobalSearchEnabledAsync(accessToken, enabled: true);

        var id = Guid.NewGuid().ToString("N");
        await SeedDocumentAsync("website", id, "news", "İstanbul Ofisi", "Özet", "ISTANBUL OFISI OZET", DateTime.UtcNow);

        var result = await SearchAsync(query);

        Assert.Contains(result.Results.Items, i => i.Url == $"/t/{id}");
    }

    [Fact]
    public async Task Search_EscapesLikeWildcards_UnderscoreDoesNotActAsAWildcard()
    {
        var accessToken = await LoginAsAdminAsync();
        await SetGlobalSearchEnabledAsync(accessToken, enabled: true);

        var literalId = Guid.NewGuid().ToString("N");
        var decoyId = Guid.NewGuid().ToString("N");
        await SeedDocumentAsync("website", literalId, "news", "A_B Kod", "Özet", "A_B KOD OZET", DateTime.UtcNow);
        await SeedDocumentAsync("website", decoyId, "news", "AXB Kod", "Özet", "AXB KOD OZET", DateTime.UtcNow);

        var result = await SearchAsync("a_b");

        var urls = result.Results.Items.Select(i => i.Url).ToList();
        Assert.Contains($"/t/{literalId}", urls);
        Assert.DoesNotContain($"/t/{decoyId}", urls);
    }

    [Fact]
    public async Task Search_RanksTitleMatchesBeforeBodyOnlyMatches_RegardlessOfPublishDate()
    {
        var accessToken = await LoginAsAdminAsync();
        await SetGlobalSearchEnabledAsync(accessToken, enabled: true);

        var token = $"benzersiz{Guid.NewGuid():N}";
        var titleMatchId = Guid.NewGuid().ToString("N");
        var bodyOnlyMatchId = Guid.NewGuid().ToString("N");

        // Older publish date but the token is in the title - must still rank first.
        await SeedDocumentAsync(
            "website", titleMatchId, "news", token.ToUpperInvariant(), "Özet", $"{token.ToUpperInvariant()} OZET",
            DateTime.UtcNow.AddDays(-10));
        await SeedDocumentAsync(
            "website", bodyOnlyMatchId, "news", "Başka Başlık", "Özet", $"BASKA BASLIK OZET {token.ToUpperInvariant()}",
            DateTime.UtcNow);

        var result = await SearchAsync(token);

        Assert.Equal(2, result.Results.Items.Count);
        Assert.Equal($"/t/{titleMatchId}", result.Results.Items[0].Url);
        Assert.Equal($"/t/{bodyOnlyMatchId}", result.Results.Items[1].Url);
    }

    [Fact]
    public async Task Search_FiltersByTypeAndSource()
    {
        var accessToken = await LoginAsAdminAsync();
        await SetGlobalSearchEnabledAsync(accessToken, enabled: true);

        var token = $"filtre{Guid.NewGuid():N}";
        var newsId = Guid.NewGuid().ToString("N");
        var jobId = Guid.NewGuid().ToString("N");
        await SeedDocumentAsync("website", newsId, "news", token, "Özet", $"{token.ToUpperInvariant()} OZET", DateTime.UtcNow);
        await SeedDocumentAsync("employer.job", jobId, "job", token, "Özet", $"{token.ToUpperInvariant()} OZET", DateTime.UtcNow);

        var typeFiltered = await _client.GetAsync($"/api/v1/public/search?q={token}&type=job");
        var typeFilteredBody = await typeFiltered.Content.ReadFromJsonAsync<PublicSearchResponse>();
        Assert.Single(typeFilteredBody!.Results.Items);
        Assert.Equal($"/t/{jobId}", typeFilteredBody.Results.Items[0].Url);

        var sourceFiltered = await _client.GetAsync($"/api/v1/public/search?q={token}&source=website");
        var sourceFilteredBody = await sourceFiltered.Content.ReadFromJsonAsync<PublicSearchResponse>();
        Assert.Single(sourceFilteredBody!.Results.Items);
        Assert.Equal($"/t/{newsId}", sourceFilteredBody.Results.Items[0].Url);
    }

    [Fact]
    public async Task Search_TypeCounts_IgnoreTheTypeFilterButRespectTheQuery()
    {
        var accessToken = await LoginAsAdminAsync();
        await SetGlobalSearchEnabledAsync(accessToken, enabled: true);

        var token = $"facet{Guid.NewGuid():N}";
        await SeedDocumentAsync(
            "website", Guid.NewGuid().ToString("N"), "news", token, "Özet", $"{token.ToUpperInvariant()} OZET", DateTime.UtcNow);
        await SeedDocumentAsync(
            "website", Guid.NewGuid().ToString("N"), "news", token, "Özet", $"{token.ToUpperInvariant()} OZET", DateTime.UtcNow);
        await SeedDocumentAsync(
            "employer.job", Guid.NewGuid().ToString("N"), "job", token, "Özet", $"{token.ToUpperInvariant()} OZET", DateTime.UtcNow);

        var response = await _client.GetAsync($"/api/v1/public/search?q={token}&type=job");
        var body = await response.Content.ReadFromJsonAsync<PublicSearchResponse>();

        Assert.Single(body!.Results.Items);
        Assert.Equal(2, body.TypeCounts["news"]);
        Assert.Equal(1, body.TypeCounts["job"]);
    }

    [Fact]
    public async Task Search_Paginates()
    {
        var accessToken = await LoginAsAdminAsync();
        await SetGlobalSearchEnabledAsync(accessToken, enabled: true);

        var token = $"sayfa{Guid.NewGuid():N}";
        for (var i = 0; i < 3; i++)
        {
            await SeedDocumentAsync(
                "website", Guid.NewGuid().ToString("N"), "news", token, "Özet", $"{token.ToUpperInvariant()} OZET",
                DateTime.UtcNow.AddMinutes(-i));
        }

        var firstPage = await _client.GetAsync($"/api/v1/public/search?q={token}&page=1&pageSize=2");
        var firstPageBody = await firstPage.Content.ReadFromJsonAsync<PublicSearchResponse>();
        Assert.Equal(2, firstPageBody!.Results.Items.Count);
        Assert.Equal(3, firstPageBody.Results.TotalCount);

        var secondPage = await _client.GetAsync($"/api/v1/public/search?q={token}&page=2&pageSize=2");
        var secondPageBody = await secondPage.Content.ReadFromJsonAsync<PublicSearchResponse>();
        Assert.Single(secondPageBody!.Results.Items);
    }

    [Fact]
    public async Task Search_ResponseNeverExposesNormalizedTextOrOtherInternalFields()
    {
        var accessToken = await LoginAsAdminAsync();
        await SetGlobalSearchEnabledAsync(accessToken, enabled: true);

        var token = $"sizdirmaz{Guid.NewGuid():N}";
        await SeedDocumentAsync(
            "website", Guid.NewGuid().ToString("N"), "news", token, "Özet metni", $"{token.ToUpperInvariant()} SIR BILGI",
            DateTime.UtcNow);

        var response = await _client.GetAsync($"/api/v1/public/search?q={token}");
        var rawJson = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("SIR BILGI", rawJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("normalizedText", rawJson, StringComparison.OrdinalIgnoreCase);
    }
}
