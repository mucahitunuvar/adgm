using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.DeleteContentItem;
using GenclikMerkezi.Modules.Website.Features.DeleteContentItemTranslation;
using GenclikMerkezi.Modules.Website.Features.GetContentItemById;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.PublishContentItem;
using GenclikMerkezi.Modules.Website.Features.RestoreContentItemRevision;
using GenclikMerkezi.Modules.Website.Features.UpdateContentItemTranslation;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §4 (Faz 5 Görev 7): content revision history - capture, retention's prerequisite
// IsPublishedSnapshot marking, listing/detail/compare reads, restore and permanent-delete cleanup.
// Shares the "one CustomWebApplicationFactory/one Sqlite database per class" caveat every other
// Website flow test class documents; reuses the seeded "news" (DetailPage, Categories, Tags) and
// "page" (Hierarchy, DetailPage, no Categories/Tags) content types.
public class ContentItemRevisionFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly CreateContentItemSeoInput EmptySeo = new(null, null, null, null, null, null, null, false);

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ContentItemRevisionFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<Guid> GetContentTypeIdByKeyAsync(string accessToken, string key)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/content-types", accessToken));
        var types = await response.Content.ReadFromJsonAsync<List<ContentTypeSummaryResponse>>();
        return types!.Single(t => t.Key == key).Id;
    }

    private async Task<ContentItemDetailResponse> GetContentItemAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{id}", accessToken));
        return (await response.Content.ReadFromJsonAsync<ContentItemDetailResponse>())!;
    }

    private async Task<Guid> CreateContentItemAsync(string accessToken, Guid contentTypeId, string title, string? summary = null)
    {
        var request = new CreateContentItemRequest(contentTypeId, null, 1, false, null, null, title, null, summary, "<p>gövde</p>", EmptySeo);
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, request));
        var created = await response.Content.ReadFromJsonAsync<CreateContentItemResponse>();
        return created!.Id;
    }

    private async Task<HttpResponseMessage> UpdateTranslationAsync(
        string accessToken, Guid id, string title, string? summary = null, string? body = "<p>gövde</p>")
    {
        var item = await GetContentItemAsync(accessToken, id);
        var seo = new UpdateContentItemTranslationSeoInput(null, null, null, null, null, null, null, false);
        return await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{id}/translations/tr", accessToken,
            new UpdateContentItemTranslationRequest(item.RowVersion, title, null, summary, body, seo)));
    }

    private async Task<HttpResponseMessage> PublishAsync(string accessToken, Guid id)
    {
        var item = await GetContentItemAsync(accessToken, id);
        return await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{id}/publish", accessToken,
            new PublishContentItemRequest(item.RowVersion, null, null)));
    }

    private async Task<JsonElement> GetRevisionsAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{id}/revisions", accessToken));
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
    }

    // --- Capture ---

    [Fact]
    public async Task CreateContentItem_RecordsFirstRevisionAsCreated()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var id = await CreateContentItemAsync(accessToken, newsTypeId, $"Haber-{Guid.NewGuid():N}");

        var root = await GetRevisionsAsync(accessToken, id);
        var items = root.GetProperty("items").EnumerateArray().ToList();

        var only = Assert.Single(items);
        Assert.Equal(1, only.GetProperty("revisionNumber").GetInt32());
        Assert.Equal("Created", only.GetProperty("kind").GetString());
        Assert.False(only.GetProperty("isPublishedSnapshot").GetBoolean());
    }

    [Fact]
    public async Task UpdateContentItemTranslation_SavingIdenticalContent_DoesNotAddRevision()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var title = $"Haber-{Guid.NewGuid():N}";
        var id = await CreateContentItemAsync(accessToken, newsTypeId, title, summary: "Özet");

        // Same title/summary/body as creation - the resulting snapshot hashes identical.
        var response = await UpdateTranslationAsync(accessToken, id, title, "Özet", "<p>gövde</p>");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var root = await GetRevisionsAsync(accessToken, id);
        Assert.Equal(1, root.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task UpdateContentItemTranslation_WithRealChange_AddsEditedRevision()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var id = await CreateContentItemAsync(accessToken, newsTypeId, $"Haber-{Guid.NewGuid():N}");

        var response = await UpdateTranslationAsync(accessToken, id, $"Yeni-Baslik-{Guid.NewGuid():N}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var root = await GetRevisionsAsync(accessToken, id);
        Assert.Equal(2, root.GetProperty("totalCount").GetInt32());
        var newest = root.GetProperty("items").EnumerateArray().First();
        Assert.Equal(2, newest.GetProperty("revisionNumber").GetInt32());
        Assert.Equal("Edited", newest.GetProperty("kind").GetString());
        Assert.Contains("tr", newest.GetProperty("changedLanguages").EnumerateArray().Select(e => e.GetString()));
    }

    [Fact]
    public async Task PublishContentItem_AddsPublishedRevisionMarkedAsSnapshot()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var id = await CreateContentItemAsync(accessToken, newsTypeId, $"Haber-{Guid.NewGuid():N}");

        var response = await PublishAsync(accessToken, id);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var root = await GetRevisionsAsync(accessToken, id);
        var newest = root.GetProperty("items").EnumerateArray().First();
        Assert.Equal("Published", newest.GetProperty("kind").GetString());
        Assert.True(newest.GetProperty("isPublishedSnapshot").GetBoolean());
    }

    [Fact]
    public async Task GetContentItemRevisions_OrdersNewestFirst()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var id = await CreateContentItemAsync(accessToken, newsTypeId, $"Haber-{Guid.NewGuid():N}");
        await UpdateTranslationAsync(accessToken, id, $"Ikinci-{Guid.NewGuid():N}");
        await UpdateTranslationAsync(accessToken, id, $"Ucuncu-{Guid.NewGuid():N}");

        var root = await GetRevisionsAsync(accessToken, id);
        var numbers = root.GetProperty("items").EnumerateArray().Select(e => e.GetProperty("revisionNumber").GetInt32()).ToList();

        Assert.Equal([3, 2, 1], numbers);
    }

    // --- Detail ---

    [Fact]
    public async Task GetContentItemRevisionByNumber_ReturnsCapturedFields()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var title = $"Haber-{Guid.NewGuid():N}";
        var id = await CreateContentItemAsync(accessToken, newsTypeId, title, summary: "Özet metni");

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{id}/revisions/1", accessToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        var translations = root.GetProperty("translations").EnumerateArray().ToList();
        var tr = Assert.Single(translations, t => t.GetProperty("languageCode").GetString() == "tr");
        Assert.Equal(title, tr.GetProperty("title").GetString());
        Assert.Equal("Özet metni", tr.GetProperty("summary").GetString());
    }

    [Fact]
    public async Task GetContentItemRevisionByNumber_UnknownNumber_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var id = await CreateContentItemAsync(accessToken, newsTypeId, $"Haber-{Guid.NewGuid():N}");

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{id}/revisions/999", accessToken));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- Compare ---

    [Fact]
    public async Task Compare_RevisionToRevision_FlagsChangedTitleOnly()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var originalTitle = $"Once-{Guid.NewGuid():N}";
        var id = await CreateContentItemAsync(accessToken, newsTypeId, originalTitle, summary: "Aynı özet");
        var updatedTitle = $"Sonra-{Guid.NewGuid():N}";
        await UpdateTranslationAsync(accessToken, id, updatedTitle, "Aynı özet");

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Get, $"/api/v1/admin/website/contents/{id}/revisions/compare?from=1&to=2&lang=tr", accessToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        var title = root.GetProperty("title");
        Assert.True(title.GetProperty("changed").GetBoolean());
        Assert.Equal(originalTitle, title.GetProperty("from").GetString());
        Assert.Equal(updatedTitle, title.GetProperty("to").GetString());

        var summary = root.GetProperty("summary");
        Assert.False(summary.GetProperty("changed").GetBoolean());
    }

    [Fact]
    public async Task Compare_RevisionToCurrent_ReflectsLiveUnsavedState()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var originalTitle = $"Once-{Guid.NewGuid():N}";
        var id = await CreateContentItemAsync(accessToken, newsTypeId, originalTitle);
        var updatedTitle = $"Sonra-{Guid.NewGuid():N}";
        await UpdateTranslationAsync(accessToken, id, updatedTitle);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Get, $"/api/v1/admin/website/contents/{id}/revisions/compare?from=1&to=current&lang=tr", accessToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal(updatedTitle, root.GetProperty("title").GetProperty("to").GetString());
    }

    // --- Restore ---

    [Fact]
    public async Task Restore_OnlyChangesSelectedLanguageAndLeavesSlugAndStatusUntouched()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var originalTitle = $"Once-{Guid.NewGuid():N}";
        var id = await CreateContentItemAsync(accessToken, newsTypeId, originalTitle);
        await PublishAsync(accessToken, id);
        await UpdateTranslationAsync(accessToken, id, $"Sonra-{Guid.NewGuid():N}");

        var beforeRestore = await GetContentItemAsync(accessToken, id);
        var slugBefore = beforeRestore.Translations.Single(t => t.LanguageCode == "tr").Slug;
        var statusBefore = beforeRestore.Status;

        var restoreResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{id}/revisions/1/restore", accessToken,
            new RestoreContentItemRevisionRequest(beforeRestore.RowVersion, "tr", false)));
        Assert.Equal(HttpStatusCode.NoContent, restoreResponse.StatusCode);

        var afterRestore = await GetContentItemAsync(accessToken, id);
        var trTranslation = afterRestore.Translations.Single(t => t.LanguageCode == "tr");
        Assert.Equal(originalTitle, trTranslation.Title);
        Assert.Equal(slugBefore, trTranslation.Slug);
        Assert.Equal(statusBefore, afterRestore.Status);
    }

    [Fact]
    public async Task Restore_RecordsANewRestoredRevision()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var id = await CreateContentItemAsync(accessToken, newsTypeId, $"Once-{Guid.NewGuid():N}");
        await UpdateTranslationAsync(accessToken, id, $"Sonra-{Guid.NewGuid():N}");
        var current = await GetContentItemAsync(accessToken, id);

        var restoreResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{id}/revisions/1/restore", accessToken,
            new RestoreContentItemRevisionRequest(current.RowVersion, "tr", false)));
        Assert.Equal(HttpStatusCode.NoContent, restoreResponse.StatusCode);

        var root = await GetRevisionsAsync(accessToken, id);
        var newest = root.GetProperty("items").EnumerateArray().First();
        Assert.Equal(3, newest.GetProperty("revisionNumber").GetInt32());
        Assert.Equal("Restored", newest.GetProperty("kind").GetString());
    }

    [Fact]
    public async Task Restore_RevisionSnapshotMissingRequestedLanguage_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var id = await CreateContentItemAsync(accessToken, newsTypeId, $"Haber-{Guid.NewGuid():N}");
        var current = await GetContentItemAsync(accessToken, id);

        // Revision #1 only ever had "tr" - "en" was never captured in it.
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{id}/revisions/1/restore", accessToken,
            new RestoreContentItemRevisionRequest(current.RowVersion, "en", false)));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Restore_LanguageNoLongerOnCurrentItem_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var pageTypeId = await GetContentTypeIdByKeyAsync(accessToken, "page");
        var id = await CreateContentItemAsync(accessToken, pageTypeId, $"Sayfa-{Guid.NewGuid():N}");

        // Create a second language's translation (so it gets its own revision with an "en" snapshot),
        // then delete that translation from the live item - but the revision still remembers it.
        var afterCreate = await GetContentItemAsync(accessToken, id);
        var enSeo = new UpdateContentItemTranslationSeoInput(null, null, null, null, null, null, null, false);
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{id}/translations/en", accessToken,
            new UpdateContentItemTranslationRequest(afterCreate.RowVersion, "English title", null, null, "<p/>", enSeo)));

        var withEnglish = await GetContentItemAsync(accessToken, id);
        var deleteResponse = await _client.SendAsync(Authorized(
            HttpMethod.Delete, $"/api/v1/admin/website/contents/{id}/translations/en", accessToken,
            new DeleteContentItemTranslationRequest(withEnglish.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDelete = await GetContentItemAsync(accessToken, id);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{id}/revisions/2/restore", accessToken,
            new RestoreContentItemRevisionRequest(afterDelete.RowVersion, "en", false)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Restore_WithStaleRowVersion_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var id = await CreateContentItemAsync(accessToken, newsTypeId, $"Haber-{Guid.NewGuid():N}");
        await UpdateTranslationAsync(accessToken, id, $"Sonra-{Guid.NewGuid():N}");
        var staleRowVersion = (await GetContentItemAsync(accessToken, id)).RowVersion;

        // A second edit moves the RowVersion on, making the one captured above stale.
        await UpdateTranslationAsync(accessToken, id, $"Daha-Sonra-{Guid.NewGuid():N}");

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{id}/revisions/1/restore", accessToken,
            new RestoreContentItemRevisionRequest(staleRowVersion, "tr", false)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // --- Permanent deletion ---

    [Fact]
    public async Task PermanentlyDeleteContentItem_RemovesAllItsRevisions()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var id = await CreateContentItemAsync(accessToken, newsTypeId, $"Haber-{Guid.NewGuid():N}");
        await UpdateTranslationAsync(accessToken, id, $"Guncellendi-{Guid.NewGuid():N}");

        var item = await GetContentItemAsync(accessToken, id);
        var trashResponse = await _client.SendAsync(Authorized(
            HttpMethod.Delete, $"/api/v1/admin/website/contents/{id}", accessToken, new DeleteContentItemRequest(item.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, trashResponse.StatusCode);

        var permanentDeleteResponse = await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/trash/{id}", accessToken));
        Assert.Equal(HttpStatusCode.NoContent, permanentDeleteResponse.StatusCode);

        // The content item itself is gone now, so GetContentItemRevisions (which 404s on an unknown
        // content item first) can no longer be used to check this - read the repository directly.
        using var scope = _factory.Services.CreateScope();
        var revisionRepository = scope.ServiceProvider.GetRequiredService<IContentItemRevisionRepository>();
        var remaining = await revisionRepository.GetRetentionRowsAsync(id, CancellationToken.None);
        Assert.Empty(remaining);
    }

    // --- Authorization ---

    [Fact]
    public async Task GetContentItemRevisions_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync($"/api/v1/admin/website/contents/{Guid.NewGuid()}/revisions");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
