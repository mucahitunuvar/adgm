using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.ConvertNotFoundPathToRedirect;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.CreateRedirect;
using GenclikMerkezi.Modules.Website.Features.GetContentItemById;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.GetNotFoundPaths;
using GenclikMerkezi.Modules.Website.Features.GetRedirects;
using GenclikMerkezi.Modules.Website.Features.UpdateContentItemTranslation;
using GenclikMerkezi.Modules.Website.Features.UpdateRedirect;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.IntegrationTests.Website;

// Redirects and NotFoundLogs are seeded fresh by each test (unique GUID-suffixed FromPath/Path
// values) - same "one CustomWebApplicationFactory/one SQLite database per class" sharing caveat as
// ContentTypeCrudFlowTests, so tests never assert against a row another test in this class produced.
public class RedirectAndNotFoundLogFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RedirectAndNotFoundLogFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<Guid> GetNewsContentTypeIdAsync(string accessToken)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/content-types", accessToken));
        var types = await response.Content.ReadFromJsonAsync<List<ContentTypeSummaryResponse>>();
        return types!.Single(t => t.Key == "news").Id;
    }

    // Creates a live, draft ContentItem (FullPathExistsAsync matches regardless of publish status -
    // ADR-024 §15) and returns (id, fullPath) so callers can collide a redirect against it. Callers
    // that also need to rename its slug (to trigger an automatic redirect) fetch RowVersion separately
    // via GetContentItemRowVersionAsync, to avoid an unconditional extra round trip for tests that
    // only need the FullPath.
    private async Task<(Guid Id, string FullPath)> CreateNewsItemAsync(string accessToken, string slug)
    {
        var contentTypeId = await GetNewsContentTypeIdAsync(accessToken);
        var emptySeo = new CreateContentItemSeoInput(null, null, null, null, null, null, null, false);
        var request = new CreateContentItemRequest(contentTypeId, null, 1, false, null, null, "Başlık", slug, null, null, emptySeo);

        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, request));
        var created = await response.Content.ReadFromJsonAsync<CreateContentItemResponse>();

        return (created!.Id, created.FullPath);
    }

    private async Task<byte[]> GetContentItemRowVersionAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{id}", accessToken));
        var body = await response.Content.ReadFromJsonAsync<ContentItemDetailResponse>();
        return body!.RowVersion;
    }

    [Fact]
    public async Task CreateRedirect_WithPathTarget_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var fromPath = $"eski-{Guid.NewGuid():N}";
        var request = new CreateRedirectRequest("tr", fromPath, "Path", null, "yeni-yol", "MovedPermanently");

        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/redirects", accessToken, request));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreateRedirectResponse>();
        Assert.Equal(fromPath, created!.FromPath);
    }

    [Fact]
    public async Task CreateRedirect_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/admin/website/redirects", new CreateRedirectRequest("tr", "eski-yol", "Path", null, "yeni-yol", "MovedPermanently"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateRedirect_AsNonAdmin_ReturnsForbidden()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email, password = "Sifre123", firstName = "Test", lastName = "User", role = "Candidate" });
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Sifre123" });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        var request = new CreateRedirectRequest("tr", $"eski-{Guid.NewGuid():N}", "Path", null, "yeni-yol", "MovedPermanently");

        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/redirects", login!.AccessToken, request));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateRedirect_WithFromPathCollidingWithLiveContentItem_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var slug = $"canli-icerik-{Guid.NewGuid():N}";
        var item = await CreateNewsItemAsync(accessToken, slug);
        var request = new CreateRedirectRequest("tr", item.FullPath, "Path", null, "baska-yol", "MovedPermanently");

        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/redirects", accessToken, request));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateRedirect_WithFromPathThatAlreadyHasARedirect_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var fromPath = $"eski-{Guid.NewGuid():N}";
        await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/redirects", accessToken,
            new CreateRedirectRequest("tr", fromPath, "Path", null, "hedef-1", "MovedPermanently")));

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/redirects", accessToken,
            new CreateRedirectRequest("tr", fromPath, "Path", null, "hedef-2", "MovedPermanently")));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateRedirect_WhenTargetPathIsItselfAnotherRedirectsFromPath_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var middlePath = $"orta-{Guid.NewGuid():N}";
        await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/redirects", accessToken,
            new CreateRedirectRequest("tr", middlePath, "Path", null, "son-yol", "MovedPermanently")));

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/redirects", accessToken,
            new CreateRedirectRequest("tr", $"ilk-{Guid.NewGuid():N}", "Path", null, middlePath, "MovedPermanently")));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateRedirect_WithTargetPathEqualToFromPath_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var path = $"kendine-{Guid.NewGuid():N}";
        var request = new CreateRedirectRequest("tr", path, "Path", null, path, "MovedPermanently");

        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/redirects", accessToken, request));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetRedirects_FiltersBySearch()
    {
        var accessToken = await LoginAsAdminAsync();
        var uniqueToken = Guid.NewGuid().ToString("N");
        var fromPath = $"aranan-{uniqueToken}";
        await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/redirects", accessToken,
            new CreateRedirectRequest("tr", fromPath, "Path", null, "hedef", "MovedPermanently")));

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Get, $"/api/v1/admin/website/redirects?search={uniqueToken}", accessToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PagedResult<RedirectResponse>>();
        var match = Assert.Single(body!.Items);
        Assert.Equal(fromPath, match.FromPath);
        Assert.False(match.IsAutomatic);
    }

    [Fact]
    public async Task UpdateRedirect_OnManualRedirect_ChangesTarget()
    {
        var accessToken = await LoginAsAdminAsync();
        var fromPath = $"eski-{Guid.NewGuid():N}";
        var createResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/redirects", accessToken,
            new CreateRedirectRequest("tr", fromPath, "Path", null, "hedef-1", "MovedPermanently")));
        var created = await createResponse.Content.ReadFromJsonAsync<CreateRedirectResponse>();

        var updateResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/redirects/{created!.Id}", accessToken,
            new UpdateRedirectRequest("Path", null, "hedef-2", "Found")));

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateRedirect_OnAutomaticRedirect_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var originalSlug = $"eski-slug-{Guid.NewGuid():N}";
        var item = await CreateNewsItemAsync(accessToken, originalSlug);
        var rowVersion = await GetContentItemRowVersionAsync(accessToken, item.Id);
        var emptyTranslationSeo = new UpdateContentItemTranslationSeoInput(null, null, null, null, null, null, null, false);
        var newSlug = $"yeni-slug-{Guid.NewGuid():N}";
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{item.Id}/translations/tr", accessToken,
            new UpdateContentItemTranslationRequest(rowVersion, "Başlık", newSlug, null, null, emptyTranslationSeo)));

        var redirectsResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, $"/api/v1/admin/website/redirects?isAutomatic=true&search={item.FullPath}", accessToken));
        var redirects = await redirectsResponse.Content.ReadFromJsonAsync<PagedResult<RedirectResponse>>();
        var automaticRedirect = Assert.Single(redirects!.Items);
        Assert.True(automaticRedirect.IsAutomatic);

        var updateResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/redirects/{automaticRedirect.Id}", accessToken,
            new UpdateRedirectRequest("Path", null, "baska-yol", "MovedPermanently")));

        Assert.Equal(HttpStatusCode.Conflict, updateResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteRedirect_OnAutomaticRedirect_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var originalSlug = $"eski-slug-{Guid.NewGuid():N}";
        var item = await CreateNewsItemAsync(accessToken, originalSlug);
        var rowVersion = await GetContentItemRowVersionAsync(accessToken, item.Id);
        var emptyTranslationSeo = new UpdateContentItemTranslationSeoInput(null, null, null, null, null, null, null, false);
        var newSlug = $"yeni-slug-{Guid.NewGuid():N}";
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{item.Id}/translations/tr", accessToken,
            new UpdateContentItemTranslationRequest(rowVersion, "Başlık", newSlug, null, null, emptyTranslationSeo)));
        var redirectsResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, $"/api/v1/admin/website/redirects?isAutomatic=true&search={item.FullPath}", accessToken));
        var redirects = await redirectsResponse.Content.ReadFromJsonAsync<PagedResult<RedirectResponse>>();
        var automaticRedirect = redirects!.Items.Single();

        var deleteResponse = await _client.SendAsync(
            Authorized(HttpMethod.Delete, $"/api/v1/admin/website/redirects/{automaticRedirect.Id}", accessToken));

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteRedirect_WithNonExistentId_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Delete, $"/api/v1/admin/website/redirects/{Guid.NewGuid()}", accessToken));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetNotFoundPaths_ReturnsSeededLogsSortedByHitCountDescending()
    {
        var accessToken = await LoginAsAdminAsync();
        var lowPath = $"az-tiklanan-{Guid.NewGuid():N}";
        var highPath = $"cok-tiklanan-{Guid.NewGuid():N}";
        await _factory.SeedNotFoundLogAsync("tr", lowPath, hitCount: 1);
        await _factory.SeedNotFoundLogAsync("tr", highPath, hitCount: 7);

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/not-found-paths", accessToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PagedResult<NotFoundLogResponse>>();
        var highIndex = body!.Items.ToList().FindIndex(i => i.Path == highPath);
        var lowIndex = body.Items.ToList().FindIndex(i => i.Path == lowPath);
        Assert.True(highIndex >= 0 && lowIndex >= 0 && highIndex < lowIndex);
    }

    [Fact]
    public async Task DeleteNotFoundPath_RemovesSeededLog()
    {
        var accessToken = await LoginAsAdminAsync();
        var id = await _factory.SeedNotFoundLogAsync("tr", $"silinecek-{Guid.NewGuid():N}");

        var response = await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/not-found-paths/{id}", accessToken));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteNotFoundPath_WithNonExistentId_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Delete, $"/api/v1/admin/website/not-found-paths/{Guid.NewGuid()}", accessToken));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ConvertNotFoundPathToRedirect_CreatesRedirectAndRemovesLogAtomically()
    {
        var accessToken = await LoginAsAdminAsync();
        var path = $"donusturulecek-{Guid.NewGuid():N}";
        var id = await _factory.SeedNotFoundLogAsync("tr", path, hitCount: 12);
        var request = new ConvertNotFoundPathToRedirectRequest("Path", null, "yeni-hedef", "MovedPermanently");

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/not-found-paths/{id}/convert-to-redirect", accessToken, request));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var converted = await response.Content.ReadFromJsonAsync<ConvertNotFoundPathToRedirectResponse>();
        Assert.Equal(path, converted!.FromPath);

        var notFoundPathsResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/not-found-paths", accessToken));
        var notFoundPaths = await notFoundPathsResponse.Content.ReadFromJsonAsync<PagedResult<NotFoundLogResponse>>();
        Assert.DoesNotContain(notFoundPaths!.Items, i => i.Id == id);

        var redirectsResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, $"/api/v1/admin/website/redirects?search={path}", accessToken));
        var redirects = await redirectsResponse.Content.ReadFromJsonAsync<PagedResult<RedirectResponse>>();
        Assert.Contains(redirects!.Items, r => r.FromPath == path);
    }

    [Fact]
    public async Task ConvertNotFoundPathToRedirect_WhenCollisionCheckFails_LeavesNotFoundLogUntouched()
    {
        var accessToken = await LoginAsAdminAsync();
        var slug = $"canli-icerik-{Guid.NewGuid():N}";
        var item = await CreateNewsItemAsync(accessToken, slug);
        var id = await _factory.SeedNotFoundLogAsync("tr", item.FullPath);
        var request = new ConvertNotFoundPathToRedirectRequest("Path", null, "baska-hedef", "MovedPermanently");

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/not-found-paths/{id}/convert-to-redirect", accessToken, request));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var notFoundPathsResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/not-found-paths", accessToken));
        var notFoundPaths = await notFoundPathsResponse.Content.ReadFromJsonAsync<PagedResult<NotFoundLogResponse>>();
        Assert.Contains(notFoundPaths!.Items, i => i.Id == id);
    }
}
