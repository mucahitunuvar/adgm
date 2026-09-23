using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.GetSiteLanguages;

namespace GenclikMerkezi.IntegrationTests.Website;

public class SiteLanguageCrudFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SiteLanguageCrudFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<List<SiteLanguageResponse>> GetLanguagesAsync(string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/admin/website/languages");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<GetSiteLanguagesResponse>();
        return body!.Items.ToList();
    }

    [Fact]
    public async Task GetLanguages_ReturnsSeededTrAsDefaultActive_AndEnAsInactive()
    {
        var accessToken = await LoginAsAdminAsync();

        var languages = await GetLanguagesAsync(accessToken);

        var turkish = Assert.Single(languages, l => l.Code == "tr");
        Assert.True(turkish.IsDefault);
        Assert.True(turkish.IsActive);

        var english = Assert.Single(languages, l => l.Code == "en");
        Assert.False(english.IsDefault);
        Assert.False(english.IsActive);
    }

    [Fact]
    public async Task GetLanguages_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/admin/website/languages");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateLanguage_ThenUpdateAndDeactivate_FullLifecycle()
    {
        var accessToken = await LoginAsAdminAsync();

        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/website/languages")
        {
            Content = JsonContent.Create(new { code = "zzt", name = "Test Dili", sortOrder = 9 }),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var id = Guid.Parse(created!["id"].ToString()!);

        var languagesAfterCreate = await GetLanguagesAsync(accessToken);
        Assert.Contains(languagesAfterCreate, l => l.Id == id && l.Name == "Test Dili" && l.IsActive && !l.IsDefault);

        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/admin/website/languages/{id}")
        {
            Content = JsonContent.Create(new { name = "Test Dili (Güncel)", sortOrder = 10 }),
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var languagesAfterUpdate = await GetLanguagesAsync(accessToken);
        Assert.Contains(languagesAfterUpdate, l => l.Id == id && l.Name == "Test Dili (Güncel)");

        var deactivateRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/admin/website/languages/{id}/deactivate");
        deactivateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deactivateResponse = await _client.SendAsync(deactivateRequest);
        Assert.Equal(HttpStatusCode.NoContent, deactivateResponse.StatusCode);

        var languagesAfterDeactivate = await GetLanguagesAsync(accessToken);
        Assert.Contains(languagesAfterDeactivate, l => l.Id == id && !l.IsActive);
    }

    [Fact]
    public async Task CreateLanguage_WithDuplicateCode_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/website/languages")
        {
            Content = JsonContent.Create(new { code = "tr", name = "Türkçe (İkinci)", sortOrder = 1 }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateLanguage_AsNonAdmin_ReturnsForbidden()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email, password = "Sifre123", firstName = "Test", lastName = "User", role = "Candidate" });
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Sifre123" });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/website/languages")
        {
            Content = JsonContent.Create(new { code = "de", name = "Deutsch", sortOrder = 3 }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeactivateDefaultLanguage_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var languages = await GetLanguagesAsync(accessToken);
        var defaultLanguage = languages.Single(l => l.IsDefault);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/admin/website/languages/{defaultLanguage.Id}/deactivate");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task SetDefault_OnActiveLanguage_SwapsDefaultAtomically()
    {
        var accessToken = await LoginAsAdminAsync();
        var originalDefaultId = (await GetLanguagesAsync(accessToken)).Single(l => l.IsDefault).Id;

        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/website/languages")
        {
            Content = JsonContent.Create(new { code = "zzu", name = "Yeni Varsayılan", sortOrder = 5 }),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var createResponse = await _client.SendAsync(createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var newLanguageId = Guid.Parse(created!["id"].ToString()!);

        var setDefaultRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/admin/website/languages/{newLanguageId}/set-default");
        setDefaultRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var setDefaultResponse = await _client.SendAsync(setDefaultRequest);
        Assert.Equal(HttpStatusCode.NoContent, setDefaultResponse.StatusCode);

        var languagesAfter = await GetLanguagesAsync(accessToken);
        Assert.Single(languagesAfter, l => l.IsDefault);
        Assert.True(languagesAfter.Single(l => l.Id == newLanguageId).IsDefault);
        Assert.False(languagesAfter.Single(l => l.Id == originalDefaultId).IsDefault);

        // Restore the original default so other tests in this shared-database fixture (e.g. those
        // asserting "tr" is currently the default) are not affected by this test's order.
        var restoreRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/admin/website/languages/{originalDefaultId}/set-default");
        restoreRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var restoreResponse = await _client.SendAsync(restoreRequest);
        Assert.Equal(HttpStatusCode.NoContent, restoreResponse.StatusCode);
    }

    [Fact]
    public async Task SetDefault_OnInactiveLanguage_ReturnsConflict_AndKeepsPreviousDefault()
    {
        var accessToken = await LoginAsAdminAsync();
        var languagesBefore = await GetLanguagesAsync(accessToken);
        var inactiveEnglish = languagesBefore.Single(l => l.Code == "en");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/admin/website/languages/{inactiveEnglish.Id}/set-default");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var languagesAfter = await GetLanguagesAsync(accessToken);
        Assert.True(languagesAfter.Single(l => l.Code == "tr").IsDefault);
    }
}
