using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.ActivateImpactMetric;
using GenclikMerkezi.Modules.Website.Features.CreateImpactMetric;
using GenclikMerkezi.Modules.Website.Features.DeactivateImpactMetric;
using GenclikMerkezi.Modules.Website.Features.DeleteImpactMetricTranslation;
using GenclikMerkezi.Modules.Website.Features.GetImpactMetricById;
using GenclikMerkezi.Modules.Website.Features.GetImpactMetrics;
using GenclikMerkezi.Modules.Website.Features.UpdateImpactMetric;
using GenclikMerkezi.Modules.Website.Features.UpdateImpactMetricTranslation;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §8.2 (Faz 2 Görev 3). Shares the "one CustomWebApplicationFactory/one Sqlite database per
// class" caveat every other Website flow test class documents.
public class ImpactMetricFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ImpactMetricFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<CreateImpactMetricResponse> CreateMetricAsync(
        string accessToken,
        decimal value = 1000,
        string? period = "2026 1. yarı",
        string? source = "Merkez iç kayıtları",
        string label = "Desteklenen Genç")
    {
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/impact-metrics", accessToken,
            new CreateImpactMetricRequest(value, "users", 1, label, "genç", period, source)));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CreateImpactMetricResponse>())!;
    }

    private async Task<ImpactMetricDetailResponse> GetMetricAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/impact-metrics/{id}", accessToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ImpactMetricDetailResponse>())!;
    }

    [Fact]
    public async Task CreateImpactMetric_WithValidInput_StartsInactive()
    {
        var accessToken = await LoginAsAdminAsync();

        var created = await CreateMetricAsync(accessToken);
        var detail = await GetMetricAsync(accessToken, created.Id);

        Assert.Equal(1000, detail.Value);
        Assert.False(detail.IsActive);
        Assert.Single(detail.Translations, t => t.LanguageCode == "tr" && t.Label == "Desteklenen Genç");
    }

    [Fact]
    public async Task CreateImpactMetric_WithNegativeValue_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/impact-metrics", accessToken,
            new CreateImpactMetricRequest(-1, null, 1, "Label", null, null, null)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Activate_WithoutDefaultLanguagePeriodOrSource_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateMetricAsync(accessToken, period: null, source: null);
        var rowVersion = (await GetMetricAsync(accessToken, created.Id)).RowVersion;

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/impact-metrics/{created.Id}/activate", accessToken,
            new ActivateImpactMetricRequest(rowVersion)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Activate_WithDefaultLanguagePeriodAndSource_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateMetricAsync(accessToken);
        var rowVersion = (await GetMetricAsync(accessToken, created.Id)).RowVersion;

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/impact-metrics/{created.Id}/activate", accessToken,
            new ActivateImpactMetricRequest(rowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var detail = await GetMetricAsync(accessToken, created.Id);
        Assert.True(detail.IsActive);
    }

    [Fact]
    public async Task UpdateTranslation_WhileActive_CannotClearDefaultLanguagePeriod()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateMetricAsync(accessToken);
        var rowVersion = (await GetMetricAsync(accessToken, created.Id)).RowVersion;
        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/impact-metrics/{created.Id}/activate", accessToken,
            new ActivateImpactMetricRequest(rowVersion)));
        var activeRowVersion = (await GetMetricAsync(accessToken, created.Id)).RowVersion;

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/impact-metrics/{created.Id}/translations/tr", accessToken,
            new UpdateImpactMetricTranslationRequest(activeRowVersion, "Desteklenen Genç", "genç", null, "Merkez iç kayıtları")));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateImpactMetric_WithStaleRowVersion_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateMetricAsync(accessToken);
        var staleRowVersion = (await GetMetricAsync(accessToken, created.Id)).RowVersion;

        var firstUpdate = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/impact-metrics/{created.Id}", accessToken,
            new UpdateImpactMetricRequest(staleRowVersion, 2000, "users", 2)));
        Assert.Equal(HttpStatusCode.NoContent, firstUpdate.StatusCode);

        var secondUpdate = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/impact-metrics/{created.Id}", accessToken,
            new UpdateImpactMetricRequest(staleRowVersion, 3000, "users", 3)));

        Assert.Equal(HttpStatusCode.Conflict, secondUpdate.StatusCode);
    }

    [Fact]
    public async Task AddUpdateAndDeleteTranslation_FullLifecycle()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateMetricAsync(accessToken);
        var rowVersion = (await GetMetricAsync(accessToken, created.Id)).RowVersion;

        var addResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/impact-metrics/{created.Id}/translations/en", accessToken,
            new UpdateImpactMetricTranslationRequest(rowVersion, "Supported Youth", "youth", null, null)));
        Assert.Equal(HttpStatusCode.NoContent, addResponse.StatusCode);

        var afterAdd = await GetMetricAsync(accessToken, created.Id);
        Assert.Equal(2, afterAdd.Translations.Count);
        Assert.Single(afterAdd.Translations, t => t.LanguageCode == "en" && t.Label == "Supported Youth");

        var deleteResponse = await _client.SendAsync(Authorized(
            HttpMethod.Delete, $"/api/v1/admin/website/impact-metrics/{created.Id}/translations/en", accessToken,
            new DeleteImpactMetricTranslationRequest(afterAdd.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDelete = await GetMetricAsync(accessToken, created.Id);
        Assert.Single(afterDelete.Translations);
    }

    [Fact]
    public async Task DeleteTranslation_ForDefaultLanguage_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateMetricAsync(accessToken);
        var rowVersion = (await GetMetricAsync(accessToken, created.Id)).RowVersion;

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Delete, $"/api/v1/admin/website/impact-metrics/{created.Id}/translations/tr", accessToken,
            new DeleteImpactMetricTranslationRequest(rowVersion)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetImpactMetrics_FiltersByIsActiveAndSearch()
    {
        var accessToken = await LoginAsAdminAsync();
        var uniqueLabel = $"Benzersiz-{Guid.NewGuid():N}";
        var created = await CreateMetricAsync(accessToken, label: uniqueLabel);

        var searchResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, $"/api/v1/admin/website/impact-metrics?search={uniqueLabel}", accessToken));
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<PagedResult<ImpactMetricSummaryResponse>>();
        Assert.Single(searchResult!.Items, i => i.Id == created.Id);

        var activeOnlyResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, $"/api/v1/admin/website/impact-metrics?search={uniqueLabel}&isActive=true", accessToken));
        var activeOnlyResult = await activeOnlyResponse.Content.ReadFromJsonAsync<PagedResult<ImpactMetricSummaryResponse>>();
        Assert.Empty(activeOnlyResult!.Items);
    }

    [Fact]
    public async Task DeactivateThenActivate_RoundTrips()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateMetricAsync(accessToken);
        var rowVersion = (await GetMetricAsync(accessToken, created.Id)).RowVersion;

        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/impact-metrics/{created.Id}/activate", accessToken,
            new ActivateImpactMetricRequest(rowVersion)));
        var afterActivate = await GetMetricAsync(accessToken, created.Id);
        Assert.True(afterActivate.IsActive);

        var deactivateResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/impact-metrics/{created.Id}/deactivate", accessToken,
            new DeactivateImpactMetricRequest(afterActivate.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, deactivateResponse.StatusCode);

        var afterDeactivate = await GetMetricAsync(accessToken, created.Id);
        Assert.False(afterDeactivate.IsActive);
    }

    [Fact]
    public async Task DeleteImpactMetric_WhenNotInUse_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateMetricAsync(accessToken);

        var deleteResponse = await _client.SendAsync(
            Authorized(HttpMethod.Delete, $"/api/v1/admin/website/impact-metrics/{created.Id}", accessToken));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDeleteResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/impact-metrics/{created.Id}", accessToken));
        Assert.Equal(HttpStatusCode.NotFound, afterDeleteResponse.StatusCode);
    }
}
