using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Application.Search;
using GenclikMerkezi.Modules.Website.Features.GetSearchSources;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §10 (Faz 5 Görev 4): GET .../search/sources and POST .../search/sources/{sourceKey}/reindex,
// both Website.Settings.Manage.
public class SearchSourceAdminEndpointsFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SearchSourceAdminEndpointsFlowTests(CustomWebApplicationFactory factory)
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

    private HttpRequestMessage Authorized(HttpMethod method, string path, string accessToken)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    [Fact]
    public async Task GetSearchSources_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/admin/website/search/sources");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ReindexSearchSource_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.SendAsync(new HttpRequestMessage(
            HttpMethod.Post, "/api/v1/admin/website/search/sources/website/reindex"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSearchSources_AlwaysIncludesWebsite_EvenBeforeAnySyncHasEverRun()
    {
        var accessToken = await LoginAsAdminAsync();

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/search/sources", accessToken));
        var sources = await response.Content.ReadFromJsonAsync<List<SearchSourceSummaryResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(sources!, s => s.SourceKey == "website");
    }

    [Fact]
    public async Task GetSearchSources_IncludesEveryRegisteredExternalSource()
    {
        var accessToken = await LoginAsAdminAsync();

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/search/sources", accessToken));
        var sources = await response.Content.ReadFromJsonAsync<List<SearchSourceSummaryResponse>>();

        // CustomWebApplicationFactory boots the real Program, so the real EmployerJobSearchSource
        // (Host's own IExternalSearchSource registration) is present even though it has never run yet.
        Assert.Contains(sources!, s => s.SourceKey == "employer.job");
    }

    [Fact]
    public async Task ReindexSearchSource_ForWebsite_RunsReconciliation_AndThenReflectsInGetSources()
    {
        var accessToken = await LoginAsAdminAsync();

        var reindexResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/search/sources/website/reindex", accessToken));
        Assert.Equal(HttpStatusCode.NoContent, reindexResponse.StatusCode);

        var getResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/search/sources", accessToken));
        var sources = await getResponse.Content.ReadFromJsonAsync<List<SearchSourceSummaryResponse>>();
        var website = sources!.Single(s => s.SourceKey == "website");

        Assert.NotNull(website.LastStartedAtUtc);
        Assert.NotNull(website.LastSucceededAtUtc);
        Assert.Null(website.LastError);
    }

    [Fact]
    public async Task ReindexSearchSource_ForUnknownSource_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/search/sources/no-such-source-{Guid.NewGuid():N}/reindex", accessToken));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ReindexSearchSource_WhenAlreadyRunningForThatSource_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();

        using var scope = _factory.Services.CreateScope();
        var coordinator = scope.ServiceProvider.GetRequiredService<SearchSourceSyncCoordinator>();
        Assert.True(coordinator.TryEnter(WebsiteSearchIndexReconciler.WebsiteSourceKey));
        try
        {
            var response = await _client.SendAsync(Authorized(
                HttpMethod.Post, "/api/v1/admin/website/search/sources/website/reindex", accessToken));

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
        finally
        {
            coordinator.Exit(WebsiteSearchIndexReconciler.WebsiteSourceKey);
        }
    }

}
