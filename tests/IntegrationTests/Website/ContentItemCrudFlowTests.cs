using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.ArchiveContentItem;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.DeleteContentItemTranslation;
using GenclikMerkezi.Modules.Website.Features.GetContentItemById;
using GenclikMerkezi.Modules.Website.Features.GetContentItems;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.PublishContentItem;
using GenclikMerkezi.Modules.Website.Features.ScheduleContentItem;
using GenclikMerkezi.Modules.Website.Features.UnarchiveContentItem;
using GenclikMerkezi.Modules.Website.Features.UnpublishContentItem;
using GenclikMerkezi.Modules.Website.Features.UpdateContentItem;
using GenclikMerkezi.Modules.Website.Features.UpdateContentItemTranslation;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.IntegrationTests.Website;

// Uses the "news" and "page" ContentTypes seeded by GenclikMerkeziContentTypeSeed (Görev 2) - "news"
// has a non-empty RoutePrefix ("haberler"), "page" has an empty one, exercising both of
// ContentItemFullPathGuard's branches. Each test creates its own uniquely-titled item so tests never
// collide with each other on FullPath uniqueness within this shared-database fixture.
public class ContentItemCrudFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly CreateContentItemSeoInput EmptySeo = new(null, null, null, null, null, null, null, false);
    private static readonly UpdateContentItemTranslationSeoInput EmptyTranslationSeo = new(null, null, null, null, null, null, null, false);

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ContentItemCrudFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<Guid> GetContentTypeIdAsync(string accessToken, string key)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/content-types", accessToken));
        var types = await response.Content.ReadFromJsonAsync<List<ContentTypeSummaryResponse>>();
        return types!.Single(t => t.Key == key).Id;
    }

    private async Task<ContentItemDetailResponse> GetContentItemByIdAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{id}", accessToken));
        var body = await response.Content.ReadFromJsonAsync<ContentItemDetailResponse>();
        return body!;
    }

    private async Task<(HttpStatusCode StatusCode, CreateContentItemResponse? Body)> CreateItemAsync(
        string accessToken, Guid contentTypeId, string title, string? body = null, Guid? parentId = null)
    {
        var request = new CreateContentItemRequest(contentTypeId, parentId, 1, false, null, null, title, null, null, body, EmptySeo);
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, request));
        var responseBody = response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<CreateContentItemResponse>() : null;
        return (response.StatusCode, responseBody);
    }

    [Fact]
    public async Task CreateContentItem_ThenGetById_ReturnsCreatedItemAsDraft()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdAsync(accessToken, "news");
        var title = $"Test Haberi {Guid.NewGuid():N}";

        var (statusCode, created) = await CreateItemAsync(accessToken, newsTypeId, title, "<p>gövde metni</p>");

        Assert.Equal(HttpStatusCode.Created, statusCode);
        var detail = await GetContentItemByIdAsync(accessToken, created!.Id);
        Assert.Equal("Draft", detail.Status);
        Assert.False(detail.IsVisible);
        Assert.Single(detail.Translations);
        Assert.Equal(title, detail.Translations[0].Title);
        Assert.Contains("gövde metni", detail.Translations[0].Body);
        Assert.StartsWith("haberler/", detail.Translations[0].FullPath);
    }

    [Fact]
    public async Task CreateContentItem_SanitizesScriptTagOutOfBody()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdAsync(accessToken, "news");
        var title = $"XSS Testi {Guid.NewGuid():N}";

        var (_, created) = await CreateItemAsync(accessToken, newsTypeId, title, "<p>merhaba</p><script>alert(1)</script>");

        var detail = await GetContentItemByIdAsync(accessToken, created!.Id);
        Assert.Contains("merhaba", detail.Translations[0].Body);
        Assert.DoesNotContain("<script", detail.Translations[0].Body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("alert", detail.Translations[0].Body);
    }

    [Fact]
    public async Task CreateContentItem_OnPageTypeWithEmptyRoutePrefix_FullPathIsJustTheSlug()
    {
        var accessToken = await LoginAsAdminAsync();
        var pageTypeId = await GetContentTypeIdAsync(accessToken, "page");
        var title = $"Test Sayfasi {Guid.NewGuid():N}";

        var (statusCode, created) = await CreateItemAsync(accessToken, pageTypeId, title);

        Assert.Equal(HttpStatusCode.Created, statusCode);
        Assert.DoesNotContain('/', created!.FullPath);
    }

    [Fact]
    public async Task CreateContentItem_OnPageTypeWithSlugCollidingWithAnotherTypesRoutePrefix_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var pageTypeId = await GetContentTypeIdAsync(accessToken, "page");
        // "haberler" is the seeded "news" type's own RoutePrefix - a root-level page item cannot
        // reuse it as its own single-segment path.
        var request = new CreateContentItemRequest(pageTypeId, null, 1, false, null, null, "haberler", "haberler", null, null, EmptySeo);

        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, request));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateContentItem_WithInvalidContentType_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var request = new CreateContentItemRequest(Guid.NewGuid(), null, 1, false, null, null, "Başlık", null, null, null, EmptySeo);

        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, request));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateContentItem_AsNonAdmin_ReturnsForbidden()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email, password = "Sifre123", firstName = "Test", lastName = "User", role = "Candidate" });
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Sifre123" });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        var adminToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdAsync(adminToken, "news");
        var request = new CreateContentItemRequest(newsTypeId, null, 1, false, null, null, "Başlık", null, null, null, EmptySeo);

        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", login!.AccessToken, request));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetContentItems_FiltersByContentTypeAndFindsCreatedItemByDefaultLanguageTitle()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdAsync(accessToken, "news");
        var title = $"Listelenecek Haber {Guid.NewGuid():N}";
        await CreateItemAsync(accessToken, newsTypeId, title);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Get, $"/api/v1/admin/website/contents?contentTypeId={newsTypeId}&search={Uri.EscapeDataString(title)}", accessToken));
        var body = await response.Content.ReadFromJsonAsync<PagedResult<ContentItemSummaryResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(body!.Items, i => i.Title == title && i.Status == "Draft");
    }

    [Fact]
    public async Task UpdateContentItem_ChangesSortOrderAndFeatured()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdAsync(accessToken, "news");
        var (_, created) = await CreateItemAsync(accessToken, newsTypeId, $"Güncellenecek {Guid.NewGuid():N}");
        var rowVersion = (await GetContentItemByIdAsync(accessToken, created!.Id)).RowVersion;

        var updateRequest = new UpdateContentItemRequest(rowVersion, 42, true, null, null);
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/contents/{created.Id}", accessToken, updateRequest));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var detail = await GetContentItemByIdAsync(accessToken, created.Id);
        Assert.Equal(42, detail.SortOrder);
        Assert.True(detail.IsFeatured);
    }

    [Fact]
    public async Task UpdateContentItem_WithStaleRowVersion_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdAsync(accessToken, "news");
        var (_, created) = await CreateItemAsync(accessToken, newsTypeId, $"Eski Surum {Guid.NewGuid():N}");
        var staleRowVersion = (await GetContentItemByIdAsync(accessToken, created!.Id)).RowVersion;

        var firstUpdate = new UpdateContentItemRequest(staleRowVersion, 1, true, null, null);
        var firstResponse = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/contents/{created.Id}", accessToken, firstUpdate));
        Assert.Equal(HttpStatusCode.NoContent, firstResponse.StatusCode);

        var secondResponse = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/contents/{created.Id}", accessToken, firstUpdate));

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateContentItemTranslation_AddsEnglishTranslation()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdAsync(accessToken, "news");
        var (_, created) = await CreateItemAsync(accessToken, newsTypeId, $"Çeviri Testi {Guid.NewGuid():N}");
        var rowVersion = (await GetContentItemByIdAsync(accessToken, created!.Id)).RowVersion;
        var englishTitle = $"Translation Test {Guid.NewGuid():N}";

        var translationRequest = new UpdateContentItemTranslationRequest(rowVersion, englishTitle, null, null, "<p>english body</p>", EmptyTranslationSeo);
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/contents/{created.Id}/translations/en", accessToken, translationRequest));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var detail = await GetContentItemByIdAsync(accessToken, created.Id);
        Assert.Equal(2, detail.Translations.Count);
        Assert.Contains(detail.Translations, t => t.LanguageCode == "en" && t.Title == englishTitle && t.FullPath.StartsWith("news/"));
    }

    [Fact]
    public async Task DeleteContentItemTranslation_DefaultLanguage_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdAsync(accessToken, "news");
        var (_, created) = await CreateItemAsync(accessToken, newsTypeId, $"Silinemez {Guid.NewGuid():N}");
        var rowVersion = (await GetContentItemByIdAsync(accessToken, created!.Id)).RowVersion;

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Delete, $"/api/v1/admin/website/contents/{created.Id}/translations/tr", accessToken,
            new DeleteContentItemTranslationRequest(rowVersion)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteContentItemTranslation_NonDefaultLanguage_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdAsync(accessToken, "news");
        var (_, created) = await CreateItemAsync(accessToken, newsTypeId, $"Once Ekle Sonra Sil {Guid.NewGuid():N}");
        var rowVersionAfterCreate = (await GetContentItemByIdAsync(accessToken, created!.Id)).RowVersion;

        var translationRequest = new UpdateContentItemTranslationRequest(
            rowVersionAfterCreate, $"To Delete {Guid.NewGuid():N}", null, null, null, EmptyTranslationSeo);
        await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/contents/{created.Id}/translations/en", accessToken, translationRequest));
        var rowVersionAfterAdd = (await GetContentItemByIdAsync(accessToken, created.Id)).RowVersion;

        var deleteResponse = await _client.SendAsync(Authorized(
            HttpMethod.Delete, $"/api/v1/admin/website/contents/{created.Id}/translations/en", accessToken,
            new DeleteContentItemTranslationRequest(rowVersionAfterAdd)));

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Single((await GetContentItemByIdAsync(accessToken, created.Id)).Translations);
    }

    [Fact]
    public async Task FullLifecycle_DraftToPublishedToUnpublishedToArchivedToUnpublished()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdAsync(accessToken, "news");
        var (_, created) = await CreateItemAsync(accessToken, newsTypeId, $"Yaşam Döngüsü {Guid.NewGuid():N}");
        var rowVersion = (await GetContentItemByIdAsync(accessToken, created!.Id)).RowVersion;

        var publishResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{created.Id}/publish", accessToken,
            new PublishContentItemRequest(rowVersion, null, null)));
        Assert.Equal(HttpStatusCode.NoContent, publishResponse.StatusCode);
        var afterPublish = await GetContentItemByIdAsync(accessToken, created.Id);
        Assert.Equal("Published", afterPublish.Status);
        Assert.True(afterPublish.IsVisible);

        var unpublishResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{created.Id}/unpublish", accessToken,
            new UnpublishContentItemRequest(afterPublish.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, unpublishResponse.StatusCode);
        var afterUnpublish = await GetContentItemByIdAsync(accessToken, created.Id);
        Assert.Equal("Unpublished", afterUnpublish.Status);
        Assert.False(afterUnpublish.IsVisible);

        var archiveResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{created.Id}/archive", accessToken,
            new ArchiveContentItemRequest(afterUnpublish.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, archiveResponse.StatusCode);
        var afterArchive = await GetContentItemByIdAsync(accessToken, created.Id);
        Assert.Equal("Archived", afterArchive.Status);

        var unarchiveResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{created.Id}/unarchive", accessToken,
            new UnarchiveContentItemRequest(afterArchive.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, unarchiveResponse.StatusCode);
        Assert.Equal("Unpublished", (await GetContentItemByIdAsync(accessToken, created.Id)).Status);
    }

    [Fact]
    public async Task Publish_TwiceInARow_SecondTimeStillSucceeds_AsRepublishFromUnpublished()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdAsync(accessToken, "news");
        var (_, created) = await CreateItemAsync(accessToken, newsTypeId, $"Yeniden Yayinla {Guid.NewGuid():N}");
        var rowVersion = (await GetContentItemByIdAsync(accessToken, created!.Id)).RowVersion;

        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{created.Id}/publish", accessToken,
            new PublishContentItemRequest(rowVersion, null, null)));
        var afterFirstPublish = await GetContentItemByIdAsync(accessToken, created.Id);
        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{created.Id}/unpublish", accessToken,
            new UnpublishContentItemRequest(afterFirstPublish.RowVersion)));
        var afterUnpublish = await GetContentItemByIdAsync(accessToken, created.Id);

        var republishResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{created.Id}/publish", accessToken,
            new PublishContentItemRequest(afterUnpublish.RowVersion, null, null)));

        Assert.Equal(HttpStatusCode.NoContent, republishResponse.StatusCode);
        Assert.Equal("Published", (await GetContentItemByIdAsync(accessToken, created.Id)).Status);
    }

    [Fact]
    public async Task Schedule_WhenNotPublished_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdAsync(accessToken, "news");
        var (_, created) = await CreateItemAsync(accessToken, newsTypeId, $"Zamanlanamaz {Guid.NewGuid():N}");
        var rowVersion = (await GetContentItemByIdAsync(accessToken, created!.Id)).RowVersion;

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{created.Id}/schedule", accessToken,
            new ScheduleContentItemRequest(rowVersion, null, DateTime.UtcNow.AddDays(1))));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateContentItem_WithDuplicateFullPath_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdAsync(accessToken, "news");
        var slug = $"ayni-slug-{Guid.NewGuid():N}";
        var firstRequest = new CreateContentItemRequest(newsTypeId, null, 1, false, null, null, "Birinci Başlık", slug, null, null, EmptySeo);
        var firstResponse = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, firstRequest));
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var secondRequest = new CreateContentItemRequest(newsTypeId, null, 1, false, null, null, "İkinci Başlık", slug, null, null, EmptySeo);
        var secondResponse = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, secondRequest));

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }
}
