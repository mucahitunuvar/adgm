using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.CreateContentType;
using GenclikMerkezi.Modules.Website.Features.GetContentItemById;
using GenclikMerkezi.Modules.Website.Features.GetContentTypeById;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.PublishContentItem;
using GenclikMerkezi.Modules.Website.Features.SetContentItemParent;
using GenclikMerkezi.Modules.Website.Features.UnpublishContentItem;
using GenclikMerkezi.Modules.Website.Features.UpdateContentTypeTranslation;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §4.3 (Faz 1a Görev 4). Only the seeded "page" type supports hierarchy, so tests that need
// two independent hierarchy-capable types (e.g. "parent must be the same content type") create their
// own via the Görev 2 API instead.
public class ContentItemHierarchyFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly CreateContentItemSeoInput EmptyItemSeo = new(null, null, null, null, null, null, null, false);
    private static readonly CreateContentTypeSeoInput EmptyTypeSeo = new(null, null, null, null, null, null, null, false);

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ContentItemHierarchyFlowTests(CustomWebApplicationFactory factory)
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
        string accessToken, Guid contentTypeId, string title, Guid? parentId = null)
    {
        var request = new CreateContentItemRequest(contentTypeId, parentId, 1, false, null, null, title, null, null, null, EmptyItemSeo);
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, request));
        var responseBody = response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<CreateContentItemResponse>() : null;
        return (response.StatusCode, responseBody);
    }

    // A fresh hierarchy-capable type with a non-empty RoutePrefix, isolated from the "page" seed type
    // (which has an empty prefix) and from other tests (unique key each call).
    private async Task<Guid> CreateHierarchyContentTypeAsync(string accessToken, string keySuffix)
    {
        var key = $"hier-{keySuffix}-{Guid.NewGuid():N}"[..30];
        var request = new CreateContentTypeRequest(
            key, "list", "page", "Manual", 1,
            SupportsHierarchy: true, SupportsCategories: false, SupportsTags: false, SupportsDetailImage: false,
            SupportsGallery: false, SupportsVideos: false, SupportsAttachments: false, SupportsEvent: false,
            SupportsBlockLayout: false, SupportsForm: false, SupportsRelatedContent: false, HasDetailPage: true,
            HasListingPage: false, IsSearchable: true, RequiresReview: false,
            DefaultLanguageName: "Test Hiyerarşi", DefaultLanguageRoutePrefix: key, Seo: EmptyTypeSeo);
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/content-types", accessToken, request));
        var created = await response.Content.ReadFromJsonAsync<CreateContentTypeResponse>();
        return created!.Id;
    }

    private async Task<byte[]> GetContentTypeRowVersionAsync(string accessToken, Guid contentTypeId)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/content-types/{contentTypeId}", accessToken));
        var detail = await response.Content.ReadFromJsonAsync<ContentTypeDetailResponse>();
        return detail!.RowVersion;
    }

    [Fact]
    public async Task CreateChild_UnderParent_NestsFullPathUnderParentsSlug()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateHierarchyContentTypeAsync(accessToken, "child-nest");
        var (_, parent) = await CreateItemAsync(accessToken, typeId, "Üst İçerik");

        var (statusCode, child) = await CreateItemAsync(accessToken, typeId, "Alt İçerik", parent!.Id);

        Assert.Equal(HttpStatusCode.Created, statusCode);
        var childDetail = await GetContentItemByIdAsync(accessToken, child!.Id);
        Assert.Equal(parent.Id, childDetail.ParentId);
        Assert.StartsWith(parent.FullPath + "/", childDetail.Translations[0].FullPath);
    }

    [Fact]
    public async Task CreateChild_WhenTypeDoesNotSupportHierarchy_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdAsync(accessToken, "news");
        var (_, parent) = await CreateItemAsync(accessToken, newsTypeId, $"Ebeveyn Olamaz {Guid.NewGuid():N}");

        var (statusCode, _) = await CreateItemAsync(accessToken, newsTypeId, $"Çocuk Olamaz {Guid.NewGuid():N}", parent!.Id);

        Assert.Equal(HttpStatusCode.BadRequest, statusCode);
    }

    [Fact]
    public async Task CreateChild_WithParentOfDifferentContentType_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeA = await CreateHierarchyContentTypeAsync(accessToken, "type-a");
        var typeB = await CreateHierarchyContentTypeAsync(accessToken, "type-b");
        var (_, parent) = await CreateItemAsync(accessToken, typeA, "Tür A Kökü");

        var (statusCode, _) = await CreateItemAsync(accessToken, typeB, "Tür B Çocuğu", parent!.Id);

        Assert.Equal(HttpStatusCode.BadRequest, statusCode);
    }

    [Fact]
    public async Task CreateGreatGrandchild_ExceedingMaxDepthOfThree_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateHierarchyContentTypeAsync(accessToken, "depth");
        var (_, root) = await CreateItemAsync(accessToken, typeId, "Derinlik 1");
        var (_, level2) = await CreateItemAsync(accessToken, typeId, "Derinlik 2", root!.Id);
        var (_, level3) = await CreateItemAsync(accessToken, typeId, "Derinlik 3", level2!.Id);

        var (statusCode, _) = await CreateItemAsync(accessToken, typeId, "Derinlik 4 Olamaz", level3!.Id);

        Assert.Equal(HttpStatusCode.BadRequest, statusCode);
    }

    [Fact]
    public async Task Publish_ChildWithUnpublishedParent_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateHierarchyContentTypeAsync(accessToken, "publish-block");
        var (_, parent) = await CreateItemAsync(accessToken, typeId, "Yayınlanmamış Ebeveyn");
        var (_, child) = await CreateItemAsync(accessToken, typeId, "Çocuk İçerik", parent!.Id);
        var childRowVersion = (await GetContentItemByIdAsync(accessToken, child!.Id)).RowVersion;

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{child.Id}/publish", accessToken,
            new PublishContentItemRequest(childRowVersion, null, null)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Publish_ChildAfterParentPublished_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateHierarchyContentTypeAsync(accessToken, "publish-ok");
        var (_, parent) = await CreateItemAsync(accessToken, typeId, "Yayınlanacak Ebeveyn");
        var (_, child) = await CreateItemAsync(accessToken, typeId, "Çocuk İçerik", parent!.Id);
        var parentRowVersion = (await GetContentItemByIdAsync(accessToken, parent!.Id)).RowVersion;

        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{parent.Id}/publish", accessToken,
            new PublishContentItemRequest(parentRowVersion, null, null)));

        var childRowVersion = (await GetContentItemByIdAsync(accessToken, child!.Id)).RowVersion;
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{child.Id}/publish", accessToken,
            new PublishContentItemRequest(childRowVersion, null, null)));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Unpublish_ParentWithPublishedChild_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateHierarchyContentTypeAsync(accessToken, "unpublish-block");
        var (_, parent) = await CreateItemAsync(accessToken, typeId, "Ebeveyn");
        var (_, child) = await CreateItemAsync(accessToken, typeId, "Çocuk", parent!.Id);

        var parentRowVersion = (await GetContentItemByIdAsync(accessToken, parent!.Id)).RowVersion;
        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{parent.Id}/publish", accessToken,
            new PublishContentItemRequest(parentRowVersion, null, null)));
        var childRowVersion = (await GetContentItemByIdAsync(accessToken, child!.Id)).RowVersion;
        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{child.Id}/publish", accessToken,
            new PublishContentItemRequest(childRowVersion, null, null)));

        var rowVersionAfterChildPublish = (await GetContentItemByIdAsync(accessToken, parent.Id)).RowVersion;
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{parent.Id}/unpublish", accessToken,
            new UnpublishContentItemRequest(rowVersionAfterChildPublish)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task SetParent_MovesItemBetweenRoots_RecomputesFullPathAndCreatesAutomaticRedirect()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateHierarchyContentTypeAsync(accessToken, "move");
        var (_, rootA) = await CreateItemAsync(accessToken, typeId, "Kök A");
        var (_, rootB) = await CreateItemAsync(accessToken, typeId, "Kök B");
        var (_, item) = await CreateItemAsync(accessToken, typeId, "Taşınan İçerik", rootA!.Id);
        var originalFullPath = (await GetContentItemByIdAsync(accessToken, item!.Id)).Translations[0].FullPath;
        Assert.StartsWith(rootA.FullPath + "/", originalFullPath);

        var rowVersion = (await GetContentItemByIdAsync(accessToken, item.Id)).RowVersion;
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{item.Id}/parent", accessToken,
            new SetContentItemParentRequest(rowVersion, rootB!.Id)));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var afterMove = await GetContentItemByIdAsync(accessToken, item.Id);
        Assert.Equal(rootB.Id, afterMove.ParentId);
        Assert.StartsWith(rootB.FullPath + "/", afterMove.Translations[0].FullPath);
        Assert.NotEqual(originalFullPath, afterMove.Translations[0].FullPath);
    }

    [Fact]
    public async Task SetParent_MovesItemWithDescendant_CascadesDescendantFullPath()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateHierarchyContentTypeAsync(accessToken, "cascade");
        var (_, rootA) = await CreateItemAsync(accessToken, typeId, "Kaynak Kök");
        var (_, rootB) = await CreateItemAsync(accessToken, typeId, "Hedef Kök");
        var (_, middle) = await CreateItemAsync(accessToken, typeId, "Orta Seviye", rootA!.Id);
        var (_, leaf) = await CreateItemAsync(accessToken, typeId, "Yaprak", middle!.Id);
        var originalLeafFullPath = (await GetContentItemByIdAsync(accessToken, leaf!.Id)).Translations[0].FullPath;

        var middleRowVersion = (await GetContentItemByIdAsync(accessToken, middle.Id)).RowVersion;
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{middle.Id}/parent", accessToken,
            new SetContentItemParentRequest(middleRowVersion, rootB!.Id)));

        var leafAfterMove = await GetContentItemByIdAsync(accessToken, leaf.Id);
        Assert.NotEqual(originalLeafFullPath, leafAfterMove.Translations[0].FullPath);
        Assert.StartsWith(rootB.FullPath + "/", leafAfterMove.Translations[0].FullPath);
    }

    [Fact]
    public async Task SetParent_ToOwnDescendant_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateHierarchyContentTypeAsync(accessToken, "cycle");
        var (_, root) = await CreateItemAsync(accessToken, typeId, "Kök");
        var (_, child) = await CreateItemAsync(accessToken, typeId, "Çocuk", root!.Id);

        var rootRowVersion = (await GetContentItemByIdAsync(accessToken, root.Id)).RowVersion;
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{root.Id}/parent", accessToken,
            new SetContentItemParentRequest(rootRowVersion, child!.Id)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateContentTypeTranslation_ChangingRoutePrefix_CascadesToExistingRootItemsAndCreatesRedirect()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateHierarchyContentTypeAsync(accessToken, "prefix-cascade");
        var (_, item) = await CreateItemAsync(accessToken, typeId, "Önek Değişince Taşınacak İçerik");
        var originalFullPath = (await GetContentItemByIdAsync(accessToken, item!.Id)).Translations[0].FullPath;

        var typeRowVersion = await GetContentTypeRowVersionAsync(accessToken, typeId);
        var newPrefix = $"yeni-onek-{Guid.NewGuid():N}"[..20];
        var translationRequest = new UpdateContentTypeTranslationRequest(
            typeRowVersion, "Test Hiyerarşi", newPrefix, new UpdateContentTypeTranslationSeoInput(null, null, null, null, null, null, null, false));
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/content-types/{typeId}/translations/tr", accessToken, translationRequest));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var afterPrefixChange = await GetContentItemByIdAsync(accessToken, item.Id);
        Assert.StartsWith(newPrefix + "/", afterPrefixChange.Translations[0].FullPath);
        Assert.NotEqual(originalFullPath, afterPrefixChange.Translations[0].FullPath);
    }
}
