using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.CreateContentPreviewLink;
using GenclikMerkezi.Modules.Website.Features.DeleteContentItem;
using GenclikMerkezi.Modules.Website.Features.DuplicateContentItem;
using GenclikMerkezi.Modules.Website.Features.GetContentItemById;
using GenclikMerkezi.Modules.Website.Features.GetContentPreview;
using GenclikMerkezi.Modules.Website.Features.GetContentTrash;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.PublishContentItem;
using GenclikMerkezi.Modules.Website.Features.SetContentItemRelatedContent;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §4.5 (Faz 1b Görev 6). Shares the "one CustomWebApplicationFactory/one Sqlite database per
// class" caveat every other Website flow test class documents - "news" is the seeded content type
// this class relies on (RelatedContent enabled, used for the permanent-deletion relation-cleanup test).
public class ContentTrashDuplicationPreviewFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ContentTrashDuplicationPreviewFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<Guid> CreateContentItemAsync(string accessToken, Guid contentTypeId, string title = "Başlık", string? slug = null)
    {
        var emptySeo = new CreateContentItemSeoInput(null, null, null, null, null, null, null, false);
        var request = new CreateContentItemRequest(
            contentTypeId, null, 1, false, null, null, title, slug ?? $"slug-{Guid.NewGuid():N}", null, null, emptySeo);
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, request));
        var created = await response.Content.ReadFromJsonAsync<CreateContentItemResponse>();
        return created!.Id;
    }

    private async Task<ContentItemDetailResponse> GetContentItemAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{id}", accessToken));
        return (await response.Content.ReadFromJsonAsync<ContentItemDetailResponse>())!;
    }

    private async Task<HttpResponseMessage> DeleteContentItemAsync(string accessToken, Guid id, byte[] rowVersion)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/admin/website/contents/{id}")
        {
            Content = JsonContent.Create(new DeleteContentItemRequest(rowVersion)),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return await _client.SendAsync(request);
    }

    private async Task PublishAsync(string accessToken, Guid id, byte[] rowVersion)
    {
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{id}/publish", accessToken,
            new PublishContentItemRequest(rowVersion, null, null)));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task UnpublishAsync(string accessToken, Guid id, byte[] rowVersion)
    {
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{id}/unpublish", accessToken, new UnpublishBody(rowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private sealed record UnpublishBody(byte[] RowVersion);

    // --- Trash ---

    [Fact]
    public async Task DeleteContentItem_WhenPublished_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        await PublishAsync(accessToken, itemId, item.RowVersion);
        var published = await GetContentItemAsync(accessToken, itemId);

        var response = await DeleteContentItemAsync(accessToken, itemId, published.RowVersion);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteContentItem_WithNonTrashedChild_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var pageTypeId = await GetContentTypeIdByKeyAsync(accessToken, "page");
        var parentId = await CreateContentItemAsync(accessToken, pageTypeId, "Üst");
        var emptySeo = new CreateContentItemSeoInput(null, null, null, null, null, null, null, false);
        var childRequest = new CreateContentItemRequest(
            pageTypeId, parentId, 1, false, null, null, "Alt", $"alt-{Guid.NewGuid():N}", null, null, emptySeo);
        await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, childRequest));
        var parent = await GetContentItemAsync(accessToken, parentId);

        var response = await DeleteContentItemAsync(accessToken, parentId, parent.RowVersion);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteContentItem_MovesToTrash_HiddenFromAdminListButVisibleInTrash()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);

        var deleteResponse = await DeleteContentItemAsync(accessToken, itemId, item.RowVersion);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var listRaw = await (await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/contents", accessToken)))
            .Content.ReadAsStringAsync();
        Assert.DoesNotContain(itemId.ToString(), listRaw);

        var trashResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/trash", accessToken));
        var trash = await trashResponse.Content.ReadFromJsonAsync<PagedResult<ContentItemTrashSummaryResponse>>();
        Assert.Contains(trash!.Items, t => t.Id == itemId);
    }

    [Fact]
    public async Task CreateContentItem_WithSameSlugAsTrashedItem_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var slug = $"kilitli-{Guid.NewGuid():N}";
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId, "Kilitli", slug);
        var item = await GetContentItemAsync(accessToken, itemId);
        await DeleteContentItemAsync(accessToken, itemId, item.RowVersion);

        var emptySeo = new CreateContentItemSeoInput(null, null, null, null, null, null, null, false);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/contents", accessToken,
            new CreateContentItemRequest(newsTypeId, null, 1, false, null, null, "Yeni", slug, null, null, emptySeo)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task RestoreContentItem_ReturnsToStatusBeforeDeletion()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        await PublishAsync(accessToken, itemId, item.RowVersion);
        var published = await GetContentItemAsync(accessToken, itemId);
        await UnpublishAsync(accessToken, itemId, published.RowVersion);
        var unpublished = await GetContentItemAsync(accessToken, itemId);
        await DeleteContentItemAsync(accessToken, itemId, unpublished.RowVersion);

        var restoreResponse = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/v1/admin/website/trash/{itemId}/restore", accessToken));
        Assert.Equal(HttpStatusCode.NoContent, restoreResponse.StatusCode);

        var restored = await GetContentItemAsync(accessToken, itemId);
        Assert.Equal("Unpublished", restored.Status);
    }

    [Fact]
    public async Task RestoreContentItem_WhenParentInTrash_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var pageTypeId = await GetContentTypeIdByKeyAsync(accessToken, "page");
        var parentId = await CreateContentItemAsync(accessToken, pageTypeId, "Üst");
        var emptySeo = new CreateContentItemSeoInput(null, null, null, null, null, null, null, false);
        var childRequest = new CreateContentItemRequest(
            pageTypeId, parentId, 1, false, null, null, "Alt", $"alt-{Guid.NewGuid():N}", null, null, emptySeo);
        var childResponse = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, childRequest));
        var childId = (await childResponse.Content.ReadFromJsonAsync<CreateContentItemResponse>())!.Id;

        var child = await GetContentItemAsync(accessToken, childId);
        await DeleteContentItemAsync(accessToken, childId, child.RowVersion);
        var parent = await GetContentItemAsync(accessToken, parentId);
        await DeleteContentItemAsync(accessToken, parentId, parent.RowVersion);

        var restoreChildResponse = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/v1/admin/website/trash/{childId}/restore", accessToken));

        Assert.Equal(HttpStatusCode.Conflict, restoreChildResponse.StatusCode);
    }

    [Fact]
    public async Task PermanentlyDeleteContentItem_WhenNotInTrash_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);

        var response = await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/trash/{itemId}", accessToken));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task PermanentlyDeleteContentItem_WithChildren_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var pageTypeId = await GetContentTypeIdByKeyAsync(accessToken, "page");
        var parentId = await CreateContentItemAsync(accessToken, pageTypeId, "Üst");
        var emptySeo = new CreateContentItemSeoInput(null, null, null, null, null, null, null, false);
        var childRequest = new CreateContentItemRequest(
            pageTypeId, parentId, 1, false, null, null, "Alt", $"alt-{Guid.NewGuid():N}", null, null, emptySeo);
        var childResponse = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, childRequest));
        var childId = (await childResponse.Content.ReadFromJsonAsync<CreateContentItemResponse>())!.Id;

        var child = await GetContentItemAsync(accessToken, childId);
        await DeleteContentItemAsync(accessToken, childId, child.RowVersion);
        var parent = await GetContentItemAsync(accessToken, parentId);
        await DeleteContentItemAsync(accessToken, parentId, parent.RowVersion);

        var permanentDeleteResponse = await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/trash/{parentId}", accessToken));

        Assert.Equal(HttpStatusCode.Conflict, permanentDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task PermanentlyDeleteContentItem_RemovesRelatedContentReferenceFromOtherItems()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var targetId = await CreateContentItemAsync(accessToken, newsTypeId, "Hedef");
        var sourceId = await CreateContentItemAsync(accessToken, newsTypeId, "Kaynak");
        var source = await GetContentItemAsync(accessToken, sourceId);
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{sourceId}/related", accessToken,
            new SetContentItemRelatedContentRequest(source.RowVersion, [targetId])));

        var target = await GetContentItemAsync(accessToken, targetId);
        await DeleteContentItemAsync(accessToken, targetId, target.RowVersion);
        var permanentDeleteResponse = await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/trash/{targetId}", accessToken));
        Assert.Equal(HttpStatusCode.NoContent, permanentDeleteResponse.StatusCode);

        var sourceAfter = await GetContentItemAsync(accessToken, sourceId);
        Assert.DoesNotContain(targetId, sourceAfter.RelatedContentItemIds);
    }

    [Fact]
    public async Task PermanentlyDeleteContentItem_Succeeds_AndItemNoLongerFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        await DeleteContentItemAsync(accessToken, itemId, item.RowVersion);

        var permanentDeleteResponse = await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/trash/{itemId}", accessToken));
        Assert.Equal(HttpStatusCode.NoContent, permanentDeleteResponse.StatusCode);

        var afterResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{itemId}", accessToken));
        Assert.Equal(HttpStatusCode.NotFound, afterResponse.StatusCode);
    }

    // --- Duplication ---

    [Fact]
    public async Task DuplicateContentItem_CopiesContentAndGeneratesSuffixedSlugAndTitle()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var originalSlug = $"orijinal-{Guid.NewGuid():N}";
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId, "Orijinal Başlık", originalSlug);

        var response = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/v1/admin/website/contents/{itemId}/duplicate", accessToken));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var duplicated = await response.Content.ReadFromJsonAsync<DuplicateContentItemResponse>();

        var duplicate = await GetContentItemAsync(accessToken, duplicated!.Id);
        var trTranslation = duplicate.Translations.Single(t => t.LanguageCode == "tr");
        Assert.Equal("Orijinal Başlık (Kopya)", trTranslation.Title);
        Assert.EndsWith($"{originalSlug}-kopya", trTranslation.Slug);
        Assert.Equal("Draft", duplicate.Status);
    }

    [Fact]
    public async Task DuplicateContentItem_CalledTwice_AppendsIncrementingSuffixOnSlugConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var originalSlug = $"tekrar-{Guid.NewGuid():N}";
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId, "Başlık", originalSlug);

        var firstResponse = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/v1/admin/website/contents/{itemId}/duplicate", accessToken));
        var secondResponse = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/v1/admin/website/contents/{itemId}/duplicate", accessToken));
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);

        var second = await LoadDuplicateAsync(secondResponse, accessToken);
        var secondTranslation = second.Translations.Single(t => t.LanguageCode == "tr");
        Assert.EndsWith($"{originalSlug}-kopya-2", secondTranslation.Slug);
    }

    private async Task<ContentItemDetailResponse> LoadDuplicateAsync(HttpResponseMessage response, string accessToken)
    {
        var duplicated = await response.Content.ReadFromJsonAsync<DuplicateContentItemResponse>();
        return await GetContentItemAsync(accessToken, duplicated!.Id);
    }

    [Fact]
    public async Task DuplicateContentItem_ForTrashedSource_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        await DeleteContentItemAsync(accessToken, itemId, item.RowVersion);

        var response = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/v1/admin/website/contents/{itemId}/duplicate", accessToken));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // --- Preview ---

    [Fact]
    public async Task CreatePreviewLink_ThenGetContentPreview_ReturnsContentAndHeaders()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId, "Önizleme Başlığı");

        var linkResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{itemId}/preview-links", accessToken,
            new CreateContentPreviewLinkRequest(null, null)));
        Assert.Equal(HttpStatusCode.OK, linkResponse.StatusCode);
        var link = await linkResponse.Content.ReadFromJsonAsync<CreateContentPreviewLinkResponse>();

        var previewResponse = await _client.GetAsync(link!.PreviewPath);

        Assert.Equal(HttpStatusCode.OK, previewResponse.StatusCode);
        Assert.Equal("no-store", previewResponse.Headers.CacheControl?.ToString());
        Assert.Equal("noindex, nofollow", string.Join(", ", previewResponse.Headers.GetValues("X-Robots-Tag")));
        var preview = await previewResponse.Content.ReadFromJsonAsync<ContentPreviewResponse>();
        Assert.Equal("Önizleme Başlığı", preview!.Title);

        // Bugfix: preview used to have no SEO block at all (ADR-024 §15's documented gap) - it now
        // shares ContentSeoResolver with the public list/detail endpoints, falling back to the title
        // when the translation's own SEO fields are empty, exactly like they do.
        Assert.NotNull(preview.Seo);
        Assert.Equal("Önizleme Başlığı", preview.Seo.MetaTitle);
    }

    [Fact]
    public async Task GetContentPreview_WithInvalidToken_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/v1/public/preview/not-a-real-token");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetContentPreview_WithTamperedToken_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);

        var linkResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{itemId}/preview-links", accessToken,
            new CreateContentPreviewLinkRequest(null, null)));
        var link = await linkResponse.Content.ReadFromJsonAsync<CreateContentPreviewLinkResponse>();
        var tampered = link!.Token[..^1] + (link.Token[^1] == 'A' ? 'B' : 'A');

        var response = await _client.GetAsync($"/api/v1/public/preview/{tampered}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetContentPreview_ForTrashedContent_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);

        var linkResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{itemId}/preview-links", accessToken,
            new CreateContentPreviewLinkRequest(null, null)));
        var link = await linkResponse.Content.ReadFromJsonAsync<CreateContentPreviewLinkResponse>();

        await DeleteContentItemAsync(accessToken, itemId, item.RowVersion);

        var response = await _client.GetAsync(link!.PreviewPath);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreatePreviewLink_WithDurationExceedingSevenDays_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{itemId}/preview-links", accessToken,
            new CreateContentPreviewLinkRequest(null, 24 * 8)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
