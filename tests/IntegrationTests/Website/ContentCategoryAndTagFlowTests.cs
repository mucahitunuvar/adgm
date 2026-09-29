using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.ActivateContentCategory;
using GenclikMerkezi.Modules.Website.Features.CreateContentCategory;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.CreateContentType;
using GenclikMerkezi.Modules.Website.Features.DeactivateContentCategory;
using GenclikMerkezi.Modules.Website.Features.DeleteContentCategoryTranslation;
using GenclikMerkezi.Modules.Website.Features.GetContentCategoriesByType;
using GenclikMerkezi.Modules.Website.Features.GetContentItemById;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.GetTags;
using GenclikMerkezi.Modules.Website.Features.MergeTag;
using GenclikMerkezi.Modules.Website.Features.SetContentItemCategories;
using GenclikMerkezi.Modules.Website.Features.SetContentItemTranslationTags;
using GenclikMerkezi.Modules.Website.Features.UpdateContentCategory;
using GenclikMerkezi.Modules.Website.Features.UpdateContentCategoryTranslation;
using GenclikMerkezi.Modules.Website.Features.UpdateTag;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §4.1 (Faz 1b Görev 3). Shares the "one CustomWebApplicationFactory/one Sqlite database per
// class" caveat every other Website flow test class documents - fixtures reuse the seeded "news"
// (Categories + Tags), "team" (Categories only, no Tags) and "page" (neither) content types.
public class ContentCategoryAndTagFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ContentCategoryAndTagFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<(Guid Id, string FullPath)> CreateContentItemAsync(string accessToken, Guid contentTypeId, string title = "Başlık")
    {
        var emptySeo = new CreateContentItemSeoInput(null, null, null, null, null, null, null, false);
        var slug = $"slug-{Guid.NewGuid():N}";
        var request = new CreateContentItemRequest(contentTypeId, null, 1, false, null, null, title, slug, null, null, emptySeo);
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, request));
        var created = await response.Content.ReadFromJsonAsync<CreateContentItemResponse>();
        return (created!.Id, created.FullPath);
    }

    private async Task<ContentItemDetailResponse> GetContentItemAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{id}", accessToken));
        return (await response.Content.ReadFromJsonAsync<ContentItemDetailResponse>())!;
    }

    private async Task<CreateContentCategoryResponse> CreateCategoryAsync(
        string accessToken, Guid typeId, Guid? parentId = null, string name = "Kültür")
    {
        var emptySeo = new CreateContentCategorySeoInput(null, null, null, null, null, null, null, false);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/content-types/{typeId}/categories", accessToken,
            new CreateContentCategoryRequest(parentId, 1, name, null, emptySeo)));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CreateContentCategoryResponse>())!;
    }

    private async Task<IReadOnlyList<ContentCategoryTreeItemResponse>> GetCategoryTreeAsync(string accessToken, Guid typeId)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/content-types/{typeId}/categories", accessToken));
        return (await response.Content.ReadFromJsonAsync<List<ContentCategoryTreeItemResponse>>())!;
    }

    [Fact]
    public async Task CreateCategory_ForTypeWithoutSupportsCategories_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var pageTypeId = await GetContentTypeIdByKeyAsync(accessToken, "page");

        var emptySeo = new CreateContentCategorySeoInput(null, null, null, null, null, null, null, false);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/content-types/{pageTypeId}/categories", accessToken,
            new CreateContentCategoryRequest(null, 1, "Kategori", null, emptySeo)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WithGrandparent_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var root = await CreateCategoryAsync(accessToken, newsTypeId, name: $"Kök-{Guid.NewGuid():N}");
        var child = await CreateCategoryAsync(accessToken, newsTypeId, root.Id, $"Alt-{Guid.NewGuid():N}");

        var emptySeo = new CreateContentCategorySeoInput(null, null, null, null, null, null, null, false);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/content-types/{newsTypeId}/categories", accessToken,
            new CreateContentCategoryRequest(child.Id, 1, $"Torun-{Guid.NewGuid():N}", null, emptySeo)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCategoryTree_ReturnsRootsWithNestedChildrenAndAssignedContentCounts()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var rootName = $"Kök-{Guid.NewGuid():N}";
        var root = await CreateCategoryAsync(accessToken, newsTypeId, name: rootName);
        var childName = $"Alt-{Guid.NewGuid():N}";
        await CreateCategoryAsync(accessToken, newsTypeId, root.Id, childName);

        var item = await CreateContentItemAsync(accessToken, newsTypeId);
        var itemDetail = await GetContentItemAsync(accessToken, item.Id);
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{item.Id}/categories", accessToken,
            new SetContentItemCategoriesRequest(itemDetail.RowVersion, [root.Id])));

        var tree = await GetCategoryTreeAsync(accessToken, newsTypeId);

        var rootNode = Assert.Single(tree, c => c.Id == root.Id);
        Assert.Equal(rootName, rootNode.Name);
        Assert.Equal(1, rootNode.AssignedContentCount);
        Assert.Single(rootNode.Children, c => c.Name == childName);
    }

    [Fact]
    public async Task UpdateCategoryTranslation_WithDuplicateSlugInSameType_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var sharedName = $"Ortak-{Guid.NewGuid():N}";
        await CreateCategoryAsync(accessToken, newsTypeId, name: sharedName);
        var second = await CreateCategoryAsync(accessToken, newsTypeId, name: $"Farkli-{Guid.NewGuid():N}");
        var secondRowVersion = (await GetCategoryTreeAsync(accessToken, newsTypeId)).Single(c => c.Id == second.Id).RowVersion;

        var emptySeo = new UpdateContentCategoryTranslationSeoInput(null, null, null, null, null, null, null, false);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/content-types/{newsTypeId}/categories/{second.Id}/translations/tr", accessToken,
            new UpdateContentCategoryTranslationRequest(secondRowVersion, sharedName, null, emptySeo)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategoryTranslation_ForDefaultLanguage_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var category = await CreateCategoryAsync(accessToken, newsTypeId, name: $"Kategori-{Guid.NewGuid():N}");
        var tree = await GetCategoryTreeAsync(accessToken, newsTypeId);
        var rowVersion = tree.Single(c => c.Id == category.Id).RowVersion;

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Delete, $"/api/v1/admin/website/content-types/{newsTypeId}/categories/{category.Id}/translations/tr", accessToken,
            new DeleteContentCategoryTranslationRequest(rowVersion)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ActivateAndDeactivateCategory_TogglesIsActive()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var category = await CreateCategoryAsync(accessToken, newsTypeId, name: $"Kategori-{Guid.NewGuid():N}");
        var rowVersion = (await GetCategoryTreeAsync(accessToken, newsTypeId)).Single(c => c.Id == category.Id).RowVersion;

        var deactivateResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/content-types/{newsTypeId}/categories/{category.Id}/deactivate", accessToken,
            new DeactivateContentCategoryRequest(rowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, deactivateResponse.StatusCode);

        var afterDeactivate = (await GetCategoryTreeAsync(accessToken, newsTypeId)).Single(c => c.Id == category.Id);
        Assert.False(afterDeactivate.IsActive);

        var activateResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/content-types/{newsTypeId}/categories/{category.Id}/activate", accessToken,
            new ActivateContentCategoryRequest(afterDeactivate.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, activateResponse.StatusCode);

        var afterActivate = (await GetCategoryTreeAsync(accessToken, newsTypeId)).Single(c => c.Id == category.Id);
        Assert.True(afterActivate.IsActive);
    }

    [Fact]
    public async Task DeleteCategory_WithChildren_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var root = await CreateCategoryAsync(accessToken, newsTypeId, name: $"Kök-{Guid.NewGuid():N}");
        await CreateCategoryAsync(accessToken, newsTypeId, root.Id, $"Alt-{Guid.NewGuid():N}");

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Delete, $"/api/v1/admin/website/content-types/{newsTypeId}/categories/{root.Id}", accessToken));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_WithAssignedContent_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var category = await CreateCategoryAsync(accessToken, newsTypeId, name: $"Kategori-{Guid.NewGuid():N}");
        var item = await CreateContentItemAsync(accessToken, newsTypeId);
        var itemDetail = await GetContentItemAsync(accessToken, item.Id);
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{item.Id}/categories", accessToken,
            new SetContentItemCategoriesRequest(itemDetail.RowVersion, [category.Id])));

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Delete, $"/api/v1/admin/website/content-types/{newsTypeId}/categories/{category.Id}", accessToken));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_WhenUnused_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var category = await CreateCategoryAsync(accessToken, newsTypeId, name: $"Kategori-{Guid.NewGuid():N}");

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Delete, $"/api/v1/admin/website/content-types/{newsTypeId}/categories/{category.Id}", accessToken));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task SetContentItemCategories_ForTypeWithoutSupportsCategories_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var pageTypeId = await GetContentTypeIdByKeyAsync(accessToken, "page");
        var item = await CreateContentItemAsync(accessToken, pageTypeId);
        var itemDetail = await GetContentItemAsync(accessToken, item.Id);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{item.Id}/categories", accessToken,
            new SetContentItemCategoriesRequest(itemDetail.RowVersion, [Guid.NewGuid()])));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetContentItemCategories_WithCategoryFromAnotherType_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var teamTypeId = await GetContentTypeIdByKeyAsync(accessToken, "team");
        var teamCategory = await CreateCategoryAsync(accessToken, teamTypeId, name: $"Ekip-{Guid.NewGuid():N}");
        var item = await CreateContentItemAsync(accessToken, newsTypeId);
        var itemDetail = await GetContentItemAsync(accessToken, item.Id);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{item.Id}/categories", accessToken,
            new SetContentItemCategoriesRequest(itemDetail.RowVersion, [teamCategory.Id])));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SetContentItemTranslationTags_FindsOrCreatesByNormalizedSlug()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var item = await CreateContentItemAsync(accessToken, newsTypeId);
        var itemDetail = await GetContentItemAsync(accessToken, item.Id);
        var uniqueTagName = $"Gençlik-{Guid.NewGuid():N}";

        var firstResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{item.Id}/translations/tr/tags", accessToken,
            new SetContentItemTranslationTagsRequest(itemDetail.RowVersion, [uniqueTagName])));
        Assert.Equal(HttpStatusCode.NoContent, firstResponse.StatusCode);

        var tagsResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/tags?search={uniqueTagName}", accessToken));
        var tags = await tagsResponse.Content.ReadFromJsonAsync<PagedResult<TagResponse>>();
        var createdTag = Assert.Single(tags!.Items, t => t.Name == uniqueTagName);
        Assert.Equal(1, createdTag.UsageCount);

        // Re-assigning the same tag with different casing must find the existing row, not create a
        // second one (ADR-024 §4.1: normalize slug matching).
        var itemAfterFirstAssign = await GetContentItemAsync(accessToken, item.Id);
        var secondResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{item.Id}/translations/tr/tags", accessToken,
            new SetContentItemTranslationTagsRequest(itemAfterFirstAssign.RowVersion, [uniqueTagName.ToLowerInvariant()])));
        Assert.Equal(HttpStatusCode.NoContent, secondResponse.StatusCode);

        var tagsAfterSecondResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/tags?search={uniqueTagName}", accessToken));
        var tagsAfterSecond = await tagsAfterSecondResponse.Content.ReadFromJsonAsync<PagedResult<TagResponse>>();
        Assert.Single(tagsAfterSecond!.Items);
    }

    [Fact]
    public async Task SetContentItemTranslationTags_ForTypeWithoutSupportsTags_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var teamTypeId = await GetContentTypeIdByKeyAsync(accessToken, "team");
        var item = await CreateContentItemAsync(accessToken, teamTypeId);
        var itemDetail = await GetContentItemAsync(accessToken, item.Id);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{item.Id}/translations/tr/tags", accessToken,
            new SetContentItemTranslationTagsRequest(itemDetail.RowVersion, ["Etiket"])));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTag_ToDuplicateSlug_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var item = await CreateContentItemAsync(accessToken, newsTypeId);
        var itemDetail = await GetContentItemAsync(accessToken, item.Id);
        var tagAName = $"TagA-{Guid.NewGuid():N}";
        var tagBName = $"TagB-{Guid.NewGuid():N}";
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{item.Id}/translations/tr/tags", accessToken,
            new SetContentItemTranslationTagsRequest(itemDetail.RowVersion, [tagAName, tagBName])));

        var tagsResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/tags?search={tagBName}", accessToken));
        var tags = await tagsResponse.Content.ReadFromJsonAsync<PagedResult<TagResponse>>();
        var tagB = tags!.Items.Single(t => t.Name == tagBName);

        var response = await _client.SendAsync(Authorized(HttpMethod.Put, $"/api/v1/admin/website/tags/{tagB.Id}", accessToken, new UpdateTagRequest(tagAName)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTag_RemovesItFromAssignedContent()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var item = await CreateContentItemAsync(accessToken, newsTypeId);
        var itemDetail = await GetContentItemAsync(accessToken, item.Id);
        var tagName = $"Silinecek-{Guid.NewGuid():N}";
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{item.Id}/translations/tr/tags", accessToken,
            new SetContentItemTranslationTagsRequest(itemDetail.RowVersion, [tagName])));

        var tagsResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/tags?search={tagName}", accessToken));
        var tag = (await tagsResponse.Content.ReadFromJsonAsync<PagedResult<TagResponse>>())!.Items.Single();

        var deleteResponse = await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/tags/{tag.Id}", accessToken));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var itemAfterDelete = await GetContentItemAsync(accessToken, item.Id);
        Assert.DoesNotContain(itemAfterDelete.Translations.Single(t => t.LanguageCode == "tr").TagIds, id => id == tag.Id);
    }

    [Fact]
    public async Task MergeTag_MovesContentToTargetTagAndRemovesSource()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var item = await CreateContentItemAsync(accessToken, newsTypeId);
        var itemDetail = await GetContentItemAsync(accessToken, item.Id);
        var sourceName = $"Kaynak-{Guid.NewGuid():N}";
        var targetName = $"Hedef-{Guid.NewGuid():N}";
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{item.Id}/translations/tr/tags", accessToken,
            new SetContentItemTranslationTagsRequest(itemDetail.RowVersion, [sourceName, targetName])));

        var sourceTagResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/tags?search={sourceName}", accessToken));
        var sourceTag = (await sourceTagResponse.Content.ReadFromJsonAsync<PagedResult<TagResponse>>())!.Items.Single();
        var targetTagResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/tags?search={targetName}", accessToken));
        var targetTag = (await targetTagResponse.Content.ReadFromJsonAsync<PagedResult<TagResponse>>())!.Items.Single();

        var mergeResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/admin/website/tags/{sourceTag.Id}/merge-into/{targetTag.Id}", accessToken));
        Assert.Equal(HttpStatusCode.NoContent, mergeResponse.StatusCode);

        var itemAfterMerge = await GetContentItemAsync(accessToken, item.Id);
        var tagIds = itemAfterMerge.Translations.Single(t => t.LanguageCode == "tr").TagIds;
        Assert.Contains(targetTag.Id, tagIds);
        Assert.DoesNotContain(sourceTag.Id, tagIds);

        var sourceAfterMergeResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/tags?search={sourceName}", accessToken));
        var sourceAfterMerge = await sourceAfterMergeResponse.Content.ReadFromJsonAsync<PagedResult<TagResponse>>();
        Assert.Empty(sourceAfterMerge!.Items);
    }
}
