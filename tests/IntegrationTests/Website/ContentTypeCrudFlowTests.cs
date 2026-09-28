using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.ActivateContentType;
using GenclikMerkezi.Modules.Website.Features.CreateContentType;
using GenclikMerkezi.Modules.Website.Features.DeactivateContentType;
using GenclikMerkezi.Modules.Website.Features.DeleteContentTypeTranslation;
using GenclikMerkezi.Modules.Website.Features.GetContentTypeById;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.UpdateContentType;
using GenclikMerkezi.Modules.Website.Features.UpdateContentTypeTranslation;

namespace GenclikMerkezi.IntegrationTests.Website;

// ContentTypes are seeded via migration (GenclikMerkeziContentTypeSeed) and shared by every test in
// this class, the same "one CustomWebApplicationFactory/one SQLite database per class" caveat as
// SiteSettingsFlowTests - tests that create their own new type (a fresh, uniquely-keyed row) avoid
// mutating the shared seed rows other tests assert against.
public class ContentTypeCrudFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly CreateContentTypeSeoInput EmptySeo = new(null, null, null, null, null, null, null, false);
    private static readonly UpdateContentTypeTranslationSeoInput EmptyTranslationSeo = new(null, null, null, null, null, null, null, false);

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ContentTypeCrudFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<List<ContentTypeSummaryResponse>> GetContentTypesAsync(string accessToken)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/content-types", accessToken));
        var body = await response.Content.ReadFromJsonAsync<List<ContentTypeSummaryResponse>>();
        return body!;
    }

    private async Task<ContentTypeDetailResponse> GetContentTypeByIdAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/content-types/{id}", accessToken));
        var body = await response.Content.ReadFromJsonAsync<ContentTypeDetailResponse>();
        return body!;
    }

    private CreateContentTypeRequest NewTypeRequest(string key, bool hasListingPage = true, string? routePrefix = null) => new(
        key, "list", "article", "PublishDateDesc", 99,
        SupportsHierarchy: false, SupportsCategories: false, SupportsTags: false, SupportsDetailImage: false,
        SupportsGallery: false, SupportsVideos: false, SupportsAttachments: false, SupportsEvent: false,
        SupportsBlockLayout: false, SupportsForm: false, SupportsRelatedContent: false, HasDetailPage: true,
        HasListingPage: hasListingPage, IsSearchable: true, RequiresReview: false,
        DefaultLanguageName: "Test Türü", DefaultLanguageRoutePrefix: routePrefix ?? key, Seo: EmptySeo);

    [Fact]
    public async Task GetContentTypes_ReturnsAllThirteenSeedTypesSortedBySortOrder()
    {
        var accessToken = await LoginAsAdminAsync();

        var types = await GetContentTypesAsync(accessToken);

        var seeded = types.Where(t => t.SortOrder is >= 1 and <= 13).OrderBy(t => t.SortOrder).ToList();
        Assert.Equal(13, seeded.Count);
        Assert.Equal("page", seeded[0].Key);
        Assert.Equal("Sayfa", seeded[0].DefaultLanguageName);
        Assert.Equal("press-release", seeded[12].Key);
        Assert.All(seeded, t => Assert.True(t.IsActive));
        Assert.All(seeded, t => Assert.Equal(0, t.ContentCount));
    }

    [Fact]
    public async Task GetContentTypes_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/admin/website/content-types");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetContentTypeById_ForSeededNewsType_ReturnsBothLanguageTranslations()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsId = (await GetContentTypesAsync(accessToken)).Single(t => t.Key == "news").Id;

        var detail = await GetContentTypeByIdAsync(accessToken, newsId);

        Assert.Equal("news", detail.Key);
        Assert.True(detail.HasListingPage);
        Assert.True(detail.SupportsTags);
        Assert.Equal(2, detail.Translations.Count);
        Assert.Contains(detail.Translations, t => t.LanguageCode == "tr" && t.Name == "Haber" && t.RoutePrefix == "haberler");
        Assert.Contains(detail.Translations, t => t.LanguageCode == "en" && t.Name == "News" && t.RoutePrefix == "news");
    }

    [Fact]
    public async Task CreateContentType_ThenGetById_ReturnsCreatedType()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = $"test-{Guid.NewGuid():N}"[..20];

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Post, "/api/v1/admin/website/content-types", accessToken, NewTypeRequest(key)));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreateContentTypeResponse>();
        Assert.Equal(key, created!.Key);
        Assert.Equal("tr", created.DefaultLanguageCode);

        var detail = await GetContentTypeByIdAsync(accessToken, created.Id);
        Assert.Equal(key, detail.Key);
        Assert.Single(detail.Translations);
        Assert.Equal(key, detail.Translations[0].RoutePrefix);
    }

    [Fact]
    public async Task CreateContentType_WithDuplicateKey_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Post, "/api/v1/admin/website/content-types", accessToken, NewTypeRequest("news", routePrefix: "ikinci-haber")));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateContentType_WithRequiresReview_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = $"test-{Guid.NewGuid():N}"[..20];
        var request = NewTypeRequest(key) with { RequiresReview = true };

        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/content-types", accessToken, request));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateContentType_WithReservedRoutePrefix_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = $"test-{Guid.NewGuid():N}"[..20];
        var request = NewTypeRequest(key, routePrefix: "admin");

        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/content-types", accessToken, request));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateContentType_WithRoutePrefixAlreadyUsedByAnotherType_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = $"test-{Guid.NewGuid():N}"[..20];
        var request = NewTypeRequest(key, routePrefix: "haberler");

        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/content-types", accessToken, request));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateContentType_AsNonAdmin_ReturnsForbidden()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email, password = "Sifre123", firstName = "Test", lastName = "User", role = "Candidate" });
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Sifre123" });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        var key = $"test-{Guid.NewGuid():N}"[..20];

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Post, "/api/v1/admin/website/content-types", login!.AccessToken, NewTypeRequest(key)));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateContentType_ChangesTemplatesSortOrderAndFlags()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = $"test-{Guid.NewGuid():N}"[..20];
        var createResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, "/api/v1/admin/website/content-types", accessToken, NewTypeRequest(key)));
        var created = await createResponse.Content.ReadFromJsonAsync<CreateContentTypeResponse>();
        var initialDetail = await GetContentTypeByIdAsync(accessToken, created!.Id);

        var updateRequest = new UpdateContentTypeRequest(
            initialDetail.RowVersion, "cards", "story", "Manual", 5,
            SupportsHierarchy: false, SupportsCategories: true, SupportsTags: false, SupportsDetailImage: false,
            SupportsGallery: false, SupportsVideos: false, SupportsAttachments: false, SupportsEvent: false,
            SupportsBlockLayout: false, SupportsForm: false, SupportsRelatedContent: false, HasDetailPage: true,
            HasListingPage: true, IsSearchable: true, RequiresReview: false);
        var updateResponse = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/content-types/{created.Id}", accessToken, updateRequest));

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
        var updatedDetail = await GetContentTypeByIdAsync(accessToken, created.Id);
        Assert.Equal("cards", updatedDetail.ListTemplate);
        Assert.Equal("story", updatedDetail.DetailTemplate);
        Assert.Equal("Manual", updatedDetail.SortMode);
        Assert.Equal(5, updatedDetail.SortOrder);
        Assert.True(updatedDetail.SupportsCategories);
    }

    [Fact]
    public async Task UpdateContentType_WithStaleRowVersion_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = $"test-{Guid.NewGuid():N}"[..20];
        var createResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, "/api/v1/admin/website/content-types", accessToken, NewTypeRequest(key)));
        var created = await createResponse.Content.ReadFromJsonAsync<CreateContentTypeResponse>();
        var staleRowVersion = (await GetContentTypeByIdAsync(accessToken, created!.Id)).RowVersion;

        var firstUpdate = new UpdateContentTypeRequest(
            staleRowVersion, "cards", "article", "Manual", 1,
            false, false, false, false, false, false, false, false, false, false, false, true, true, true, false);
        var firstResponse = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/content-types/{created.Id}", accessToken, firstUpdate));
        Assert.Equal(HttpStatusCode.NoContent, firstResponse.StatusCode);

        var secondResponse = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/content-types/{created.Id}", accessToken, firstUpdate));

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateContentTypeTranslation_AddsEnglishTranslationToNewType()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = $"test-{Guid.NewGuid():N}"[..20];
        var createResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, "/api/v1/admin/website/content-types", accessToken, NewTypeRequest(key)));
        var created = await createResponse.Content.ReadFromJsonAsync<CreateContentTypeResponse>();
        var rowVersion = (await GetContentTypeByIdAsync(accessToken, created!.Id)).RowVersion;

        var translationRequest = new UpdateContentTypeTranslationRequest(rowVersion, "Test Type", $"{key}-en", EmptyTranslationSeo);
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/content-types/{created.Id}/translations/en", accessToken, translationRequest));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var detail = await GetContentTypeByIdAsync(accessToken, created.Id);
        Assert.Equal(2, detail.Translations.Count);
        Assert.Contains(detail.Translations, t => t.LanguageCode == "en" && t.Name == "Test Type" && t.RoutePrefix == $"{key}-en");
    }

    [Fact]
    public async Task DeleteContentTypeTranslation_DefaultLanguage_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsId = (await GetContentTypesAsync(accessToken)).Single(t => t.Key == "news").Id;
        var rowVersion = (await GetContentTypeByIdAsync(accessToken, newsId)).RowVersion;
        var url = $"/api/v1/admin/website/content-types/{newsId}/translations/tr";

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Delete, url, accessToken, new DeleteContentTypeTranslationRequest(rowVersion)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteContentTypeTranslation_NonDefaultLanguage_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = $"test-{Guid.NewGuid():N}"[..20];
        var createResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, "/api/v1/admin/website/content-types", accessToken, NewTypeRequest(key)));
        var created = await createResponse.Content.ReadFromJsonAsync<CreateContentTypeResponse>();
        var rowVersionAfterCreate = (await GetContentTypeByIdAsync(accessToken, created!.Id)).RowVersion;

        var translationRequest = new UpdateContentTypeTranslationRequest(rowVersionAfterCreate, "Test Type", $"{key}-en", EmptyTranslationSeo);
        await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/content-types/{created.Id}/translations/en", accessToken, translationRequest));
        var rowVersionAfterAdd = (await GetContentTypeByIdAsync(accessToken, created.Id)).RowVersion;

        var url = $"/api/v1/admin/website/content-types/{created.Id}/translations/en";
        var deleteResponse = await _client.SendAsync(
            Authorized(HttpMethod.Delete, url, accessToken, new DeleteContentTypeTranslationRequest(rowVersionAfterAdd)));

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        var detail = await GetContentTypeByIdAsync(accessToken, created.Id);
        Assert.Single(detail.Translations);
    }

    [Fact]
    public async Task ActivateDeactivateContentType_TogglesIsActive()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = $"test-{Guid.NewGuid():N}"[..20];
        var createResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, "/api/v1/admin/website/content-types", accessToken, NewTypeRequest(key)));
        var created = await createResponse.Content.ReadFromJsonAsync<CreateContentTypeResponse>();
        var rowVersion = (await GetContentTypeByIdAsync(accessToken, created!.Id)).RowVersion;

        var deactivateResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/content-types/{created.Id}/deactivate", accessToken,
            new DeactivateContentTypeRequest(rowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, deactivateResponse.StatusCode);
        Assert.False((await GetContentTypeByIdAsync(accessToken, created.Id)).IsActive);

        var rowVersionAfterDeactivate = (await GetContentTypeByIdAsync(accessToken, created.Id)).RowVersion;
        var activateResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/content-types/{created.Id}/activate", accessToken,
            new ActivateContentTypeRequest(rowVersionAfterDeactivate)));
        Assert.Equal(HttpStatusCode.NoContent, activateResponse.StatusCode);
        Assert.True((await GetContentTypeByIdAsync(accessToken, created.Id)).IsActive);
    }
}
