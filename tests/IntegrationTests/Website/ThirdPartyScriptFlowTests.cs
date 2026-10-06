using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.ActivateThirdPartyScript;
using GenclikMerkezi.Modules.Website.Features.CreateThirdPartyScript;
using GenclikMerkezi.Modules.Website.Features.DeactivateThirdPartyScript;
using GenclikMerkezi.Modules.Website.Features.DeleteThirdPartyScriptTranslation;
using GenclikMerkezi.Modules.Website.Features.GetPublicSite;
using GenclikMerkezi.Modules.Website.Features.GetThirdPartyScriptById;
using GenclikMerkezi.Modules.Website.Features.GetThirdPartyScripts;
using GenclikMerkezi.Modules.Website.Features.UpdateThirdPartyScript;
using GenclikMerkezi.Modules.Website.Features.UpdateThirdPartyScriptTranslation;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §13 (Faz 3 Görev 7). Shares the "one CustomWebApplicationFactory/one Sqlite database per
// class" caveat every other Website flow test class documents. CustomWebApplicationFactory configures
// "www.googletagmanager.com" as the only Website:Scripts:AllowedScriptHosts entry.
public class ThirdPartyScriptFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ThirdPartyScriptFlowTests(CustomWebApplicationFactory factory)
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

    private static CreateThirdPartyScriptRequest Ga4Request(int sortOrder = 1, string name = "Google Analytics") =>
        new("GoogleAnalytics4", "G-ABCD1234", null, null, null, false, false, "Analytics", "Head", sortOrder, name, "Ziyaretçi istatistikleri");

    private async Task<CreateThirdPartyScriptResponse> CreateScriptAsync(string accessToken, CreateThirdPartyScriptRequest request)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/third-party-scripts", accessToken, request));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CreateThirdPartyScriptResponse>())!;
    }

    private async Task<ThirdPartyScriptDetailResponse> GetScriptAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/third-party-scripts/{id}", accessToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ThirdPartyScriptDetailResponse>())!;
    }

    [Fact]
    public async Task CreateThirdPartyScript_WithValidGoogleAnalytics4_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();

        var created = await CreateScriptAsync(accessToken, Ga4Request());
        var detail = await GetScriptAsync(accessToken, created.Id);

        Assert.Equal("GoogleAnalytics4", detail.Provider);
        Assert.Equal("G-ABCD1234", detail.MeasurementId);
        Assert.Equal("Analytics", detail.Category);
        Assert.Equal("Head", detail.Placement);
        Assert.True(detail.IsActive);
        Assert.Single(detail.Translations, t => t.LanguageCode == "tr" && t.Name == "Google Analytics");
    }

    [Fact]
    public async Task CreateThirdPartyScript_WithInvalidMeasurementId_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/third-party-scripts", accessToken,
            new CreateThirdPartyScriptRequest(
                "GoogleAnalytics4", "not-a-measurement-id", null, null, null, false, false, "Analytics", "Head", 1, "GA4", "Purpose")));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateThirdPartyScript_WithExternalScriptOnAllowedHost_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();

        var created = await CreateScriptAsync(accessToken, new CreateThirdPartyScriptRequest(
            "ExternalScript", null, null, null, "https://www.googletagmanager.com/gtag/js", true, false, "Marketing", "BodyEnd", 1,
            "GTM Script", "Reklam etiketleri"));
        var detail = await GetScriptAsync(accessToken, created.Id);

        Assert.Equal("ExternalScript", detail.Provider);
        Assert.Equal("https://www.googletagmanager.com/gtag/js", detail.Src);
        Assert.True(detail.Async);
    }

    [Fact]
    public async Task CreateThirdPartyScript_WithExternalScriptHostNotAllowed_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/third-party-scripts", accessToken,
            new CreateThirdPartyScriptRequest(
                "ExternalScript", null, null, null, "https://evil.example.com/script.js", false, false, "Marketing", "BodyEnd", 1,
                "Kötü script", "Açıklama")));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateThirdPartyScript_WithUnrecognizedProvider_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/third-party-scripts", accessToken,
            new CreateThirdPartyScriptRequest(
                "RawHtml", null, null, null, "<script>alert(1)</script>", false, false, "Marketing", "BodyEnd", 1, "Zararlı", "Açıklama")));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateThirdPartyScript_WithStaleRowVersion_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateScriptAsync(accessToken, Ga4Request());
        var staleRowVersion = (await GetScriptAsync(accessToken, created.Id)).RowVersion;

        var firstUpdate = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/third-party-scripts/{created.Id}", accessToken,
            new UpdateThirdPartyScriptRequest(staleRowVersion, "GoogleAnalytics4", "G-WXYZ9999", null, null, null, false, false, "Analytics", "Head", 2)));
        Assert.Equal(HttpStatusCode.NoContent, firstUpdate.StatusCode);

        var secondUpdate = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/third-party-scripts/{created.Id}", accessToken,
            new UpdateThirdPartyScriptRequest(staleRowVersion, "GoogleAnalytics4", "G-WXYZ9999", null, null, null, false, false, "Analytics", "Head", 3)));

        Assert.Equal(HttpStatusCode.Conflict, secondUpdate.StatusCode);
    }

    [Fact]
    public async Task AddUpdateAndDeleteTranslation_FullLifecycle()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateScriptAsync(accessToken, Ga4Request());
        var rowVersion = (await GetScriptAsync(accessToken, created.Id)).RowVersion;

        var addResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/third-party-scripts/{created.Id}/translations/en", accessToken,
            new UpdateThirdPartyScriptTranslationRequest(rowVersion, "Google Analytics", "Visitor statistics")));
        Assert.Equal(HttpStatusCode.NoContent, addResponse.StatusCode);

        var afterAdd = await GetScriptAsync(accessToken, created.Id);
        Assert.Equal(2, afterAdd.Translations.Count);

        var deleteResponse = await _client.SendAsync(Authorized(
            HttpMethod.Delete, $"/api/v1/admin/website/third-party-scripts/{created.Id}/translations/en", accessToken,
            new DeleteThirdPartyScriptTranslationRequest(afterAdd.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDelete = await GetScriptAsync(accessToken, created.Id);
        Assert.Single(afterDelete.Translations);
    }

    [Fact]
    public async Task DeleteTranslation_ForDefaultLanguage_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateScriptAsync(accessToken, Ga4Request());
        var rowVersion = (await GetScriptAsync(accessToken, created.Id)).RowVersion;

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Delete, $"/api/v1/admin/website/third-party-scripts/{created.Id}/translations/tr", accessToken,
            new DeleteThirdPartyScriptTranslationRequest(rowVersion)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Activate_ReactivatesADeactivatedScript()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateScriptAsync(accessToken, Ga4Request());
        var rowVersion = (await GetScriptAsync(accessToken, created.Id)).RowVersion;

        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/third-party-scripts/{created.Id}/deactivate", accessToken,
            new DeactivateThirdPartyScriptRequest(rowVersion)));
        var afterDeactivate = await GetScriptAsync(accessToken, created.Id);
        Assert.False(afterDeactivate.IsActive);

        var activateResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/third-party-scripts/{created.Id}/activate", accessToken,
            new ActivateThirdPartyScriptRequest(afterDeactivate.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, activateResponse.StatusCode);

        var afterActivate = await GetScriptAsync(accessToken, created.Id);
        Assert.True(afterActivate.IsActive);
    }

    [Fact]
    public async Task DeleteThirdPartyScript_WhenExists_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateScriptAsync(accessToken, Ga4Request());

        var deleteResponse = await _client.SendAsync(
            Authorized(HttpMethod.Delete, $"/api/v1/admin/website/third-party-scripts/{created.Id}", accessToken));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDeleteResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/third-party-scripts/{created.Id}", accessToken));
        Assert.Equal(HttpStatusCode.NotFound, afterDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task GetThirdPartyScripts_FiltersByIsActiveAndCategory()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateScriptAsync(accessToken, Ga4Request(name: $"Benzersiz-{Guid.NewGuid():N}"));
        var rowVersion = (await GetScriptAsync(accessToken, created.Id)).RowVersion;
        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/third-party-scripts/{created.Id}/deactivate", accessToken,
            new DeactivateThirdPartyScriptRequest(rowVersion)));

        var categoryResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, "/api/v1/admin/website/third-party-scripts?category=Analytics", accessToken));
        var categoryResult = await categoryResponse.Content.ReadFromJsonAsync<PagedResult<ThirdPartyScriptSummaryResponse>>();
        Assert.Contains(categoryResult!.Items, i => i.Id == created.Id);

        var activeOnlyResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, "/api/v1/admin/website/third-party-scripts?isActive=true", accessToken));
        var activeOnlyResult = await activeOnlyResponse.Content.ReadFromJsonAsync<PagedResult<ThirdPartyScriptSummaryResponse>>();
        Assert.DoesNotContain(activeOnlyResult!.Items, i => i.Id == created.Id);
    }

    [Fact]
    public async Task PublicSite_OnlyReturnsActiveScriptsWithATranslationInTheRequestedLanguage()
    {
        var accessToken = await LoginAsAdminAsync();
        var activeName = $"Aktif-{Guid.NewGuid():N}";
        var active = await CreateScriptAsync(accessToken, Ga4Request(name: activeName));

        var inactive = await CreateScriptAsync(accessToken, Ga4Request(name: $"Pasif-{Guid.NewGuid():N}"));
        var inactiveRowVersion = (await GetScriptAsync(accessToken, inactive.Id)).RowVersion;
        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/third-party-scripts/{inactive.Id}/deactivate", accessToken,
            new DeactivateThirdPartyScriptRequest(inactiveRowVersion)));

        var publicResponse = await _client.GetAsync("/api/v1/public/site?lang=tr");
        Assert.Equal(HttpStatusCode.OK, publicResponse.StatusCode);
        var publicSite = await publicResponse.Content.ReadFromJsonAsync<PublicSiteResponse>();

        Assert.Contains(publicSite!.Scripts, s => s.Id == active.Id);
        Assert.DoesNotContain(publicSite.Scripts, s => s.Id == inactive.Id);

        var analyticsCategory = Assert.Single(publicSite.CookieConsent.Categories, c => c.Category == "Analytics");
        Assert.Contains(analyticsCategory.Scripts, s => s.Name == activeName);
    }
}
