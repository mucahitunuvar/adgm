using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.CreateContentCategory;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.GetContentItemById;
using GenclikMerkezi.Modules.Website.Features.GetContentItems;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.GetTags;
using GenclikMerkezi.Modules.Website.Features.PublishContentItem;
using GenclikMerkezi.Modules.Website.Features.SetContentItemCategories;
using GenclikMerkezi.Modules.Website.Features.SetContentItemTranslationTags;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.IntegrationTests.Website;

// Covers the task's added GetContentItems filters (categoryId, tagId, publishedFrom/ToUtc,
// createdFrom/ToUtc, orderBy/direction) against the real EF Core/SQL Server provider - the unit
// tests in UnitTests/Website/Features/GetContentItems cover the same filters against the in-memory
// fake, but only a real provider run can confirm EF Core actually translates the JSON-array
// `CategoryIds`/`TagIds` `Contains` calls and the `PublishAtUtc ?? PublishedAtUtc` coalesce into SQL
// that executes (a LINQ expression that is valid against LINQ-to-Objects can still throw
// "could not be translated" against a real provider).
public class GetContentItemsFilterFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly CreateContentItemSeoInput EmptySeo = new(null, null, null, null, null, null, null, false);
    private static readonly CreateContentCategorySeoInput EmptyCategorySeo = new(null, null, null, null, null, null, null, false);

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GetContentItemsFilterFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<Guid> GetNewsContentTypeIdAsync(string accessToken)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/content-types", accessToken));
        var types = await response.Content.ReadFromJsonAsync<List<ContentTypeSummaryResponse>>();
        return types!.Single(t => t.Key == "news").Id;
    }

    private async Task<CreateContentItemResponse> CreateItemAsync(string accessToken, Guid contentTypeId, string title)
    {
        var request = new CreateContentItemRequest(contentTypeId, null, 1, false, null, null, title, null, null, null, EmptySeo);
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, request));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CreateContentItemResponse>())!;
    }

    private async Task<byte[]> GetRowVersionAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{id}", accessToken));
        return (await response.Content.ReadFromJsonAsync<ContentItemDetailResponse>())!.RowVersion;
    }

    private async Task<Guid> CreateCategoryAsync(string accessToken, Guid contentTypeId, Guid? parentId, string name, string slug)
    {
        var request = new CreateContentCategoryRequest(parentId, 1, name, slug, EmptyCategorySeo);
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/admin/website/content-types/{contentTypeId}/categories", accessToken, request));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CreateContentCategoryResponse>())!.Id;
    }

    private async Task SetCategoriesAsync(string accessToken, Guid itemId, IReadOnlyList<Guid> categoryIds)
    {
        var rowVersion = await GetRowVersionAsync(accessToken, itemId);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/categories", accessToken,
            new SetContentItemCategoriesRequest(rowVersion, categoryIds)));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<Guid> SetTagAsync(string accessToken, Guid itemId, string tagName)
    {
        var rowVersion = await GetRowVersionAsync(accessToken, itemId);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/translations/tr/tags", accessToken,
            new SetContentItemTranslationTagsRequest(rowVersion, [tagName])));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var tagsResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, $"/api/v1/admin/website/tags?languageCode=tr&search={Uri.EscapeDataString(tagName)}", accessToken));
        var tags = await tagsResponse.Content.ReadFromJsonAsync<PagedResult<TagResponse>>();
        return tags!.Items.Single(t => t.Name == tagName).Id;
    }

    private async Task PublishAsync(string accessToken, Guid itemId)
    {
        var rowVersion = await GetRowVersionAsync(accessToken, itemId);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{itemId}/publish", accessToken,
            new PublishContentItemRequest(rowVersion, null, null)));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<PagedResult<ContentItemSummaryResponse>> GetContentItemsAsync(string accessToken, string queryString)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents?{queryString}", accessToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<PagedResult<ContentItemSummaryResponse>>())!;
    }

    [Fact]
    public async Task GetContentItems_WithCategoryId_IncludesItemsInDirectChildCategoryToo()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetNewsContentTypeIdAsync(accessToken);
        var suffix = Guid.NewGuid().ToString("N");
        var rootCategoryId = await CreateCategoryAsync(accessToken, newsTypeId, null, $"Kök {suffix}", $"kok-{suffix}");
        var childCategoryId = await CreateCategoryAsync(accessToken, newsTypeId, rootCategoryId, $"Alt {suffix}", $"alt-{suffix}");
        var otherCategoryId = await CreateCategoryAsync(accessToken, newsTypeId, null, $"Diğer {suffix}", $"diger-{suffix}");

        var inRoot = await CreateItemAsync(accessToken, newsTypeId, $"Kökte {suffix}");
        await SetCategoriesAsync(accessToken, inRoot.Id, [rootCategoryId]);
        var inChild = await CreateItemAsync(accessToken, newsTypeId, $"Altta {suffix}");
        await SetCategoriesAsync(accessToken, inChild.Id, [childCategoryId]);
        var inOther = await CreateItemAsync(accessToken, newsTypeId, $"Diğerinde {suffix}");
        await SetCategoriesAsync(accessToken, inOther.Id, [otherCategoryId]);

        var body = await GetContentItemsAsync(accessToken, $"contentTypeId={newsTypeId}&categoryId={rootCategoryId}");

        Assert.Equal(2, body.Items.Count(i => i.Id == inRoot.Id || i.Id == inChild.Id));
        Assert.DoesNotContain(body.Items, i => i.Id == inOther.Id);
    }

    [Fact]
    public async Task GetContentItems_WithNonExistentCategoryId_ReturnsEmptyNotUnfiltered()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetNewsContentTypeIdAsync(accessToken);
        await CreateItemAsync(accessToken, newsTypeId, $"Herhangi {Guid.NewGuid():N}");

        var body = await GetContentItemsAsync(accessToken, $"contentTypeId={newsTypeId}&categoryId={Guid.NewGuid()}");

        Assert.Equal(0, body.TotalCount);
    }

    [Fact]
    public async Task GetContentItems_WithTagId_FiltersToTaggedItem()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetNewsContentTypeIdAsync(accessToken);
        var suffix = Guid.NewGuid().ToString("N");
        var tagged = await CreateItemAsync(accessToken, newsTypeId, $"Etiketli {suffix}");
        var tagId = await SetTagAsync(accessToken, tagged.Id, $"etiket-{suffix}");
        var untagged = await CreateItemAsync(accessToken, newsTypeId, $"Etiketsiz {suffix}");

        var body = await GetContentItemsAsync(accessToken, $"contentTypeId={newsTypeId}&tagId={tagId}");

        Assert.Contains(body.Items, i => i.Id == tagged.Id);
        Assert.DoesNotContain(body.Items, i => i.Id == untagged.Id);
    }

    [Fact]
    public async Task GetContentItems_WithPublishedDateRange_IsHalfOpenAndUsesEffectivePublishDate()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetNewsContentTypeIdAsync(accessToken);
        var suffix = Guid.NewGuid().ToString("N");
        var item = await CreateItemAsync(accessToken, newsTypeId, $"Hemen Yayınla {suffix}");
        var beforePublish = DateTime.UtcNow;
        await PublishAsync(accessToken, item.Id); // immediate publish -> PublishedAtUtc set, PublishAtUtc stays null.
        var afterPublish = DateTime.UtcNow.AddSeconds(1);

        var excluding = await GetContentItemsAsync(
            accessToken,
            $"contentTypeId={newsTypeId}&publishedFromUtc={Uri.EscapeDataString(beforePublish.ToString("O"))}" +
                $"&publishedToUtc={Uri.EscapeDataString(beforePublish.ToString("O"))}");
        Assert.DoesNotContain(excluding.Items, i => i.Id == item.Id);

        var including = await GetContentItemsAsync(
            accessToken,
            $"contentTypeId={newsTypeId}&publishedFromUtc={Uri.EscapeDataString(beforePublish.ToString("O"))}" +
                $"&publishedToUtc={Uri.EscapeDataString(afterPublish.ToString("O"))}");
        Assert.Contains(including.Items, i => i.Id == item.Id);
    }

    [Fact]
    public async Task GetContentItems_WithCreatedDateRange_IsHalfOpen()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetNewsContentTypeIdAsync(accessToken);
        var suffix = Guid.NewGuid().ToString("N");
        var beforeCreate = DateTime.UtcNow;
        var item = await CreateItemAsync(accessToken, newsTypeId, $"Oluşturuldu {suffix}");
        var afterCreate = DateTime.UtcNow.AddSeconds(1);

        var excluding = await GetContentItemsAsync(
            accessToken,
            $"contentTypeId={newsTypeId}&createdFromUtc={Uri.EscapeDataString(beforeCreate.ToString("O"))}" +
                $"&createdToUtc={Uri.EscapeDataString(beforeCreate.ToString("O"))}");
        Assert.DoesNotContain(excluding.Items, i => i.Id == item.Id);

        var including = await GetContentItemsAsync(
            accessToken,
            $"contentTypeId={newsTypeId}&createdFromUtc={Uri.EscapeDataString(beforeCreate.ToString("O"))}" +
                $"&createdToUtc={Uri.EscapeDataString(afterCreate.ToString("O"))}");
        Assert.Contains(including.Items, i => i.Id == item.Id);
    }

    [Fact]
    public async Task GetContentItems_WithOrderByTitleDescending_SortsAccordingly()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetNewsContentTypeIdAsync(accessToken);
        var suffix = Guid.NewGuid().ToString("N");
        var a = await CreateItemAsync(accessToken, newsTypeId, $"A Sıralama {suffix}");
        var b = await CreateItemAsync(accessToken, newsTypeId, $"B Sıralama {suffix}");

        var body = await GetContentItemsAsync(
            accessToken, $"contentTypeId={newsTypeId}&search={Uri.EscapeDataString(suffix)}&orderBy=Title&direction=Desc");

        var indexA = body.Items.ToList().FindIndex(i => i.Id == a.Id);
        var indexB = body.Items.ToList().FindIndex(i => i.Id == b.Id);
        Assert.True(indexB < indexA, "B Sıralama should sort before A Sıralama in descending title order.");
    }

    [Theory]
    [InlineData("orderBy=NotAField")]
    [InlineData("orderBy=Title&direction=Sideways")]
    public async Task GetContentItems_WithInvalidOrderByOrDirection_ReturnsBadRequest(string queryString)
    {
        var accessToken = await LoginAsAdminAsync();

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents?{queryString}", accessToken));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("publishedFromUtc=2026-01-02T00:00:00Z&publishedToUtc=2026-01-01T00:00:00Z")]
    [InlineData("createdFromUtc=2026-01-02T00:00:00Z&createdToUtc=2026-01-01T00:00:00Z")]
    public async Task GetContentItems_WithInvertedDateRange_ReturnsBadRequest(string queryString)
    {
        var accessToken = await LoginAsAdminAsync();

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents?{queryString}", accessToken));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
