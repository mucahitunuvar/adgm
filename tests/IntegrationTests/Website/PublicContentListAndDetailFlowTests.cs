using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.CreateContentCategory;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.GetContentCategoriesByType;
using GenclikMerkezi.Modules.Website.Features.GetContentItemById;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.GetPublicContentById;
using GenclikMerkezi.Modules.Website.Features.GetPublicContents;
using GenclikMerkezi.Modules.Website.Features.PublishContentItem;
using GenclikMerkezi.Modules.Website.Features.SetContentItemCategories;
using GenclikMerkezi.Modules.Website.Features.UpdateContentItemTranslation;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §17 (Faz 1b Görev 7). Shares the "one CustomWebApplicationFactory/one Sqlite database per
// class" caveat every other Website flow test class documents. Reuses the seeded "news" (Categories +
// Tags + Gallery + Videos + Attachments + RelatedContent + DetailPage + ListingPage), "page"
// (Hierarchy + DetailPage, no ListingPage), "document" (Categories + Attachments + ListingPage, no
// DetailPage) and "press-release" (ListingPage only - neither Categories nor Tags) content types.
//
// Deliberately not covered here: a scheduled item becoming visible purely from PublishAtUtc's clock
// elapsing (the master prompt's "TTL shortening with time mocked" scenario) - CustomWebApplicationFactory
// wires the real system TimeProvider for every module, and adding a seam to fake it here would be a
// shared-fixture change well beyond this test class's scope. ContentCacheTtlCalculatorTests already
// covers the TTL-shortening math in isolation; GetPublicContentById_AfterPublishing_BecomesVisible below
// covers that a cache miss immediately reflects a real publish.
public class PublicContentListAndDetailFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PublicContentListAndDetailFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<(Guid Id, string FullPath)> CreateContentItemAsync(
        string accessToken, Guid contentTypeId, Guid? parentId, string title = "Başlık", string? body = null, string? summary = null)
    {
        var emptySeo = new CreateContentItemSeoInput(null, null, null, null, null, null, null, false);
        var slug = $"slug-{Guid.NewGuid():N}";
        var request = new CreateContentItemRequest(contentTypeId, parentId, 1, false, null, null, title, slug, summary, body, emptySeo);
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, request));
        var created = await response.Content.ReadFromJsonAsync<CreateContentItemResponse>();
        return (created!.Id, created.FullPath);
    }

    private async Task PublishAsync(string accessToken, Guid id, DateTime? publishAtUtc = null)
    {
        var rowVersion = (await GetContentItemAsync(accessToken, id)).RowVersion;
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{id}/publish", accessToken,
            new PublishContentItemRequest(rowVersion, publishAtUtc, null)));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<CreateContentCategoryResponse> CreateCategoryAsync(string accessToken, Guid typeId, Guid? parentId, string name)
    {
        var emptySeo = new CreateContentCategorySeoInput(null, null, null, null, null, null, null, false);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/content-types/{typeId}/categories", accessToken,
            new CreateContentCategoryRequest(parentId, 1, name, null, emptySeo)));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CreateContentCategoryResponse>())!;
    }

    private async Task<string> GetCategorySlugAsync(string accessToken, Guid typeId, Guid categoryId)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/content-types/{typeId}/categories", accessToken));
        var tree = await response.Content.ReadFromJsonAsync<List<ContentCategoryTreeItemResponse>>();
        foreach (var root in tree!)
        {
            if (root.Id == categoryId)
            {
                return root.Slug;
            }

            var child = root.Children.FirstOrDefault(c => c.Id == categoryId);
            if (child is not null)
            {
                return child.Slug;
            }
        }

        throw new InvalidOperationException($"Category '{categoryId}' not found in tree.");
    }

    private async Task AssignCategoryAsync(string accessToken, Guid itemId, Guid categoryId)
    {
        var item = await GetContentItemAsync(accessToken, itemId);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/categories", accessToken,
            new SetContentItemCategoriesRequest(item.RowVersion, [categoryId])));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<HttpResponseMessage> GetListAsync(string queryString) =>
        await _client.GetAsync($"/api/v1/public/contents{queryString}");

    private async Task<HttpResponseMessage> GetDetailAsync(Guid id, string? lang = null) =>
        await _client.GetAsync($"/api/v1/public/contents/{id}" + (lang is null ? string.Empty : $"?lang={lang}"));

    // --- List: type-level validation ---

    [Fact]
    public async Task GetPublicContents_UnknownType_ReturnsNotFound()
    {
        var response = await GetListAsync($"?type=bilinmeyen-{Guid.NewGuid():N}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicContents_TypeWithoutListingPage_ReturnsNotFound()
    {
        // "page" has DetailPage but no ListingPage.
        var response = await GetListAsync("?type=page");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicContents_CategoryFilterOnUnsupportedType_ReturnsBadRequest()
    {
        // "press-release" has ListingPage but neither Categories nor Tags.
        var response = await GetListAsync("?type=press-release&category=herhangi-bir-slug");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicContents_TagFilterOnUnsupportedType_ReturnsBadRequest()
    {
        var response = await GetListAsync("?type=press-release&tag=herhangi-bir-slug");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicContents_SearchTooShort_ReturnsBadRequest()
    {
        var response = await GetListAsync("?type=news&search=a");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicContents_UnknownCategorySlug_ReturnsEmptyResult()
    {
        var response = await GetListAsync($"?type=news&category=yok-boyle-bir-kategori-{Guid.NewGuid():N}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PublicContentListResponse>();
        Assert.Empty(body!.Items.Items);
    }

    // --- List: filtering, sorting, pagination ---

    [Fact]
    public async Task GetPublicContents_CategoryFilter_IncludesChildCategoryItems()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var root = await CreateCategoryAsync(accessToken, newsTypeId, null, $"Kok-{Guid.NewGuid():N}");
        var child = await CreateCategoryAsync(accessToken, newsTypeId, root.Id, $"Alt-{Guid.NewGuid():N}");
        var rootSlug = await GetCategorySlugAsync(accessToken, newsTypeId, root.Id);

        var item = await CreateContentItemAsync(accessToken, newsTypeId, null, "Alt kategori haberi");
        await AssignCategoryAsync(accessToken, item.Id, child.Id);
        await PublishAsync(accessToken, item.Id);

        var response = await GetListAsync($"?type=news&category={rootSlug}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PublicContentListResponse>();
        Assert.Contains(body!.Items.Items, i => i.Id == item.Id);
    }

    [Fact]
    public async Task GetPublicContents_NewsSortMode_OrdersByEffectivePublishDateDescending()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var older = await CreateContentItemAsync(accessToken, newsTypeId, null, $"Eski-{Guid.NewGuid():N}");
        var newer = await CreateContentItemAsync(accessToken, newsTypeId, null, $"Yeni-{Guid.NewGuid():N}");
        await PublishAsync(accessToken, older.Id, DateTime.UtcNow.AddDays(-2));
        await PublishAsync(accessToken, newer.Id, DateTime.UtcNow.AddDays(-1));

        var response = await GetListAsync("?type=news&pageSize=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PublicContentListResponse>();
        var newerIndex = body!.Items.Items.ToList().FindIndex(i => i.Id == newer.Id);
        var olderIndex = body.Items.Items.ToList().FindIndex(i => i.Id == older.Id);
        Assert.True(newerIndex >= 0 && olderIndex >= 0 && newerIndex < olderIndex);
    }

    [Fact]
    public async Task GetPublicContents_TypeWithoutDetailPage_ReturnsBodyInlineWithNullPath()
    {
        var accessToken = await LoginAsAdminAsync();
        var documentTypeId = await GetContentTypeIdByKeyAsync(accessToken, "document");
        var item = await CreateContentItemAsync(
            accessToken, documentTypeId, null, $"Belge-{Guid.NewGuid():N}", body: "<p>Belge içeriği</p>");
        await PublishAsync(accessToken, item.Id);

        var response = await GetListAsync("?type=document");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PublicContentListResponse>();
        var listed = Assert.Single(body!.Items.Items, i => i.Id == item.Id);
        Assert.Null(listed.Path);
        Assert.Equal("<p>Belge içeriği</p>", listed.Body);
    }

    [Fact]
    public async Task GetPublicContents_TypeWithDetailPage_ReturnsPathWithoutInlineBody()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var item = await CreateContentItemAsync(accessToken, newsTypeId, null, $"Haber-{Guid.NewGuid():N}", body: "<p>Gövde</p>");
        await PublishAsync(accessToken, item.Id);

        var response = await GetListAsync("?type=news");

        var body = await response.Content.ReadFromJsonAsync<PublicContentListResponse>();
        var listed = Assert.Single(body!.Items.Items, i => i.Id == item.Id);
        Assert.NotNull(listed.Path);
        Assert.Null(listed.Body);
    }

    [Fact]
    public async Task GetPublicContents_PageSizeAboveFifty_IsCappedAtFifty()
    {
        var response = await GetListAsync("?type=news&pageSize=200");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PublicContentListResponse>();
        Assert.Equal(50, body!.Items.PageSize);
    }

    // --- List: visibility rule ---

    // Regression test: SearchPublicListAsync used to check only Status == Published, never
    // PublishAtUtc/UnpublishAtUtc, so a scheduled (future) or already-expired item still appeared in
    // the public list even though the same rule (ContentItem.IsVisible) already hid it from the detail
    // endpoint and from related content.
    [Fact]
    public async Task GetPublicContents_ScheduledOrExpiredItems_AreExcludedFromList()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");

        var scheduledTitle = $"Zamanlanmis-{Guid.NewGuid():N}";
        var scheduled = await CreateContentItemAsync(accessToken, newsTypeId, null, scheduledTitle);
        await PublishAsync(accessToken, scheduled.Id, DateTime.UtcNow.AddDays(1));

        var expiredTitle = $"SuresiDolmus-{Guid.NewGuid():N}";
        var expired = await CreateContentItemAsync(accessToken, newsTypeId, null, expiredTitle);
        var expiredItem = await GetContentItemAsync(accessToken, expired.Id);
        var publishResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{expired.Id}/publish", accessToken,
            new PublishContentItemRequest(expiredItem.RowVersion, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1))));
        Assert.Equal(HttpStatusCode.NoContent, publishResponse.StatusCode);

        var response = await GetListAsync("?type=news&pageSize=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PublicContentListResponse>();
        Assert.DoesNotContain(body!.Items.Items, i => i.Id == scheduled.Id);
        Assert.DoesNotContain(body.Items.Items, i => i.Id == expired.Id);
    }

    // --- List: cache invalidation ---

    [Fact]
    public async Task GetPublicContents_AfterUpdatingTitle_ReflectsChangeOnNextRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var originalTitle = $"Once-{Guid.NewGuid():N}";
        var item = await CreateContentItemAsync(accessToken, newsTypeId, null, originalTitle);
        await PublishAsync(accessToken, item.Id);

        var firstResponse = await GetListAsync("?type=news");
        var firstBody = await firstResponse.Content.ReadFromJsonAsync<PublicContentListResponse>();
        Assert.Contains(firstBody!.Items.Items, i => i.Id == item.Id && i.Title == originalTitle);

        var updatedTitle = $"Sonra-{Guid.NewGuid():N}";
        var itemDetail = await GetContentItemAsync(accessToken, item.Id);
        var emptyTranslationSeo = new UpdateContentItemTranslationSeoInput(null, null, null, null, null, null, null, false);
        var updateResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{item.Id}/translations/tr", accessToken,
            new UpdateContentItemTranslationRequest(itemDetail.RowVersion, updatedTitle, null, null, null, emptyTranslationSeo)));
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var secondResponse = await GetListAsync("?type=news");
        var secondBody = await secondResponse.Content.ReadFromJsonAsync<PublicContentListResponse>();
        Assert.Contains(secondBody!.Items.Items, i => i.Id == item.Id && i.Title == updatedTitle);
    }

    // --- Detail: not-found paths ---

    [Fact]
    public async Task GetPublicContentById_UnknownId_ReturnsNotFound()
    {
        var response = await GetDetailAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicContentById_TypeWithoutDetailPage_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var teamTypeId = await GetContentTypeIdByKeyAsync(accessToken, "team");
        var item = await CreateContentItemAsync(accessToken, teamTypeId, null);
        await PublishAsync(accessToken, item.Id);

        var response = await GetDetailAsync(item.Id);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicContentById_ChildUnderScheduledParent_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var pageTypeId = await GetContentTypeIdByKeyAsync(accessToken, "page");
        var parent = await CreateContentItemAsync(accessToken, pageTypeId, null, $"Zamanlanmis-Ebeveyn-{Guid.NewGuid():N}");
        // Publish() only requires the parent's Status to already be Published, not that it is
        // currently visible (ADR-024 §4.4) - a future PublishAtUtc lets the child publish while the
        // parent itself is not yet visible, exactly like ResolveRouteFlowTests's equivalent case.
        await PublishAsync(accessToken, parent.Id, DateTime.UtcNow.AddDays(7));
        var child = await CreateContentItemAsync(accessToken, pageTypeId, parent.Id, $"Cocuk-{Guid.NewGuid():N}");
        await PublishAsync(accessToken, child.Id);

        var response = await GetDetailAsync(child.Id);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicContentById_DraftItem_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var item = await CreateContentItemAsync(accessToken, newsTypeId, null);

        var response = await GetDetailAsync(item.Id);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- Detail: full response shape ---

    [Fact]
    public async Task GetPublicContentById_PublishedNewsItem_ReturnsFullyPopulatedResponse()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var category = await CreateCategoryAsync(accessToken, newsTypeId, null, $"Kategori-{Guid.NewGuid():N}");
        var title = $"Haber-{Guid.NewGuid():N}";
        var item = await CreateContentItemAsync(accessToken, newsTypeId, null, title, body: "<p>Gövde metni</p>", summary: "Özet metni");
        await AssignCategoryAsync(accessToken, item.Id, category.Id);
        await PublishAsync(accessToken, item.Id);

        var response = await GetDetailAsync(item.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var detail = await response.Content.ReadFromJsonAsync<PublicContentDetailResponse>();
        Assert.Equal(item.Id, detail!.Id);
        Assert.Equal("news", detail.ContentTypeKey);
        Assert.Equal(title, detail.Title);
        Assert.Equal("<p>Gövde metni</p>", detail.Body);
        Assert.Equal("/" + item.FullPath, detail.Path);
        Assert.Contains(detail.Categories, c => c.Id == category.Id);
        Assert.Empty(detail.Gallery);
        Assert.Empty(detail.Videos);
        Assert.Empty(detail.Attachments);
        Assert.Empty(detail.Children);
        Assert.NotNull(detail.Seo);
        Assert.NotEmpty(detail.Seo.MetaTitle);

        // Breadcrumb: Home -> type listing page -> item itself ("news" HasListingPage = true).
        Assert.Equal(3, detail.Breadcrumb.Count);
        Assert.Equal("/", detail.Breadcrumb[0].Path);
        Assert.Equal("/haberler", detail.Breadcrumb[1].Path);
        Assert.Equal(detail.Path, detail.Breadcrumb[2].Path);
    }

    [Fact]
    public async Task GetPublicContentById_HierarchicalTypeWithoutListingPage_BreadcrumbExcludesListingEntry()
    {
        var accessToken = await LoginAsAdminAsync();
        var pageTypeId = await GetContentTypeIdByKeyAsync(accessToken, "page");
        var parentTitle = $"Ebeveyn-{Guid.NewGuid():N}";
        var parent = await CreateContentItemAsync(accessToken, pageTypeId, null, parentTitle);
        await PublishAsync(accessToken, parent.Id);
        var childTitle = $"Cocuk-{Guid.NewGuid():N}";
        var child = await CreateContentItemAsync(accessToken, pageTypeId, parent.Id, childTitle);
        await PublishAsync(accessToken, child.Id);

        var response = await GetDetailAsync(child.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var detail = await response.Content.ReadFromJsonAsync<PublicContentDetailResponse>();

        // "page" has no ListingPage: breadcrumb is Home -> parent -> child, never a type-listing entry.
        Assert.Equal(3, detail!.Breadcrumb.Count);
        Assert.Equal("/", detail.Breadcrumb[0].Path);
        Assert.Equal(parentTitle, detail.Breadcrumb[1].Title);
        Assert.Equal(childTitle, detail.Breadcrumb[2].Title);
        Assert.Equal(detail.Path, detail.Breadcrumb[2].Path);
    }

    [Fact]
    public async Task GetPublicContentById_HierarchicalParent_ListsVisibleChildInChildrenSection()
    {
        var accessToken = await LoginAsAdminAsync();
        var pageTypeId = await GetContentTypeIdByKeyAsync(accessToken, "page");
        var parent = await CreateContentItemAsync(accessToken, pageTypeId, null, $"Ebeveyn-{Guid.NewGuid():N}");
        await PublishAsync(accessToken, parent.Id);
        var visibleChildTitle = $"GorunurCocuk-{Guid.NewGuid():N}";
        var visibleChild = await CreateContentItemAsync(accessToken, pageTypeId, parent.Id, visibleChildTitle);
        await PublishAsync(accessToken, visibleChild.Id);
        var draftChild = await CreateContentItemAsync(accessToken, pageTypeId, parent.Id, $"TaslakCocuk-{Guid.NewGuid():N}");

        var response = await GetDetailAsync(parent.Id);

        var detail = await response.Content.ReadFromJsonAsync<PublicContentDetailResponse>();
        Assert.Contains(detail!.Children, c => c.Id == visibleChild.Id);
        Assert.DoesNotContain(detail.Children, c => c.Id == draftChild.Id);
    }

    // --- Detail: cache invalidation ---

    [Fact]
    public async Task GetPublicContentById_AfterPublishing_BecomesVisible()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var item = await CreateContentItemAsync(accessToken, newsTypeId, null);

        var beforePublish = await GetDetailAsync(item.Id);
        Assert.Equal(HttpStatusCode.NotFound, beforePublish.StatusCode);

        await PublishAsync(accessToken, item.Id);

        var afterPublish = await GetDetailAsync(item.Id);
        Assert.Equal(HttpStatusCode.OK, afterPublish.StatusCode);
    }

    [Fact]
    public async Task GetPublicContentById_AfterUpdatingTitle_ReflectsChangeOnNextRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var originalTitle = $"Once-{Guid.NewGuid():N}";
        var item = await CreateContentItemAsync(accessToken, newsTypeId, null, originalTitle);
        await PublishAsync(accessToken, item.Id);

        var firstResponse = await GetDetailAsync(item.Id);
        var firstDetail = await firstResponse.Content.ReadFromJsonAsync<PublicContentDetailResponse>();
        Assert.Equal(originalTitle, firstDetail!.Title);

        var updatedTitle = $"Sonra-{Guid.NewGuid():N}";
        var itemDetail = await GetContentItemAsync(accessToken, item.Id);
        var emptyTranslationSeo = new UpdateContentItemTranslationSeoInput(null, null, null, null, null, null, null, false);
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{item.Id}/translations/tr", accessToken,
            new UpdateContentItemTranslationRequest(itemDetail.RowVersion, updatedTitle, null, null, null, emptyTranslationSeo)));

        var secondResponse = await GetDetailAsync(item.Id);
        var secondDetail = await secondResponse.Content.ReadFromJsonAsync<PublicContentDetailResponse>();
        Assert.Equal(updatedTitle, secondDetail!.Title);
    }
}
