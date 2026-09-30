using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.DeleteContentItem;
using GenclikMerkezi.Modules.Website.Features.GetContentItemById;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.GetMenuByLocation;
using GenclikMerkezi.Modules.Website.Features.PublishContentItem;
using GenclikMerkezi.Modules.Website.Features.ReplaceMenuItems;
using GenclikMerkezi.Modules.Website.Features.UnpublishContentItem;

namespace GenclikMerkezi.IntegrationTests.Website;

// Faz 2 Görev 1. Shares the "one CustomWebApplicationFactory/one Sqlite database per class" caveat
// every other Website flow test class documents: all three Menu rows (Header/Utility/Footer) are
// seeded once by migration and shared across every test below. Each test either targets a location no
// other test in this class touches, or fetches the current RowVersion immediately before mutating -
// so tests remain order-independent without needing to assume any prior empty state.
public class MenuFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public MenuFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<MenuResponse> GetMenuAsync(string accessToken, string location)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/menus/{location}", accessToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<MenuResponse>())!;
    }

    private async Task<HttpResponseMessage> ReplaceMenuAsync(
        string accessToken, string location, byte[] rowVersion, IReadOnlyList<MenuItemTreeInput> items) =>
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/menus/{location}", accessToken, new ReplaceMenuItemsRequest(rowVersion, items)));

    private static MenuItemTreeInput Group(string tempId) => new(
        tempId, null, 1, true, null, false, null, [new MenuItemTranslationInput("tr", "Grup " + tempId)]);

    private static MenuItemTreeInput Leaf(
        string tempId, string? parentTempId, MenuItemLinkInput? link, bool isActive = true, string label = "Etiket") => new(
        tempId, parentTempId, 1, isActive, link, false, null, [new MenuItemTranslationInput("tr", label)]);

    private async Task<Guid> GetNewsContentTypeIdAsync(string accessToken)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/content-types", accessToken));
        var types = await response.Content.ReadFromJsonAsync<List<ContentTypeSummaryResponse>>();
        return types!.Single(t => t.Key == "news").Id;
    }

    private async Task<Guid> CreateAndPublishNewsItemAsync(string accessToken)
    {
        var contentTypeId = await GetNewsContentTypeIdAsync(accessToken);
        var slug = $"haber-{Guid.NewGuid():N}"[..24];
        var createRequest = new CreateContentItemRequest(
            contentTypeId, null, 1, false, null, null, "Test Haberi " + slug, slug, "Özet", "<p>Gövde</p>",
            new CreateContentItemSeoInput(null, null, null, null, null, null, null, false));
        var createResponse = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, createRequest));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = (await createResponse.Content.ReadFromJsonAsync<CreateContentItemResponse>())!;

        var detail = await GetContentItemAsync(accessToken, created.Id);
        var publishResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{created.Id}/publish", accessToken,
            new PublishContentItemRequest(detail.RowVersion, null, null)));
        Assert.Equal(HttpStatusCode.NoContent, publishResponse.StatusCode);

        return created.Id;
    }

    private async Task<ContentItemDetailResponse> GetContentItemAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{id}", accessToken));
        return (await response.Content.ReadFromJsonAsync<ContentItemDetailResponse>())!;
    }

    private async Task<PublicMenusOnlyResponse> GetPublicMenusAsync()
    {
        var response = await _client.GetAsync("/api/v1/public/site?lang=tr");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<PublicSiteMenusOnlyResponse>())!.Menus;
    }

    [Fact]
    public async Task GetMenus_ReturnsAllThreeSeededLocations()
    {
        var accessToken = await LoginAsAdminAsync();

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/menus", accessToken));
        var menus = await response.Content.ReadFromJsonAsync<List<MenuResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, menus!.Count);
        Assert.Contains(menus, m => m.Location == "Header");
        Assert.Contains(menus, m => m.Location == "Utility");
        Assert.Contains(menus, m => m.Location == "Footer");
    }

    [Fact]
    public async Task ReplaceMenuItems_WithValidTree_IsReflectedInAdminAndPublicResponses()
    {
        var accessToken = await LoginAsAdminAsync();
        var current = await GetMenuAsync(accessToken, "Header");

        var putResponse = await ReplaceMenuAsync(
            accessToken, "Header", current.RowVersion,
            [
                Group("g1"),
                Leaf("c1", "g1", new MenuItemLinkInput("InternalPath", null, null, "/kampanya", null), label: "Kampanya"),
                Leaf("c2", null, new MenuItemLinkInput("ExternalUrl", null, null, null, "https://example.com"), label: "Dış Link"),
            ]);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        var adminMenu = await GetMenuAsync(accessToken, "Header");
        Assert.Equal(3, adminMenu.Items.Count);
        Assert.All(adminMenu.Items, i => Assert.Null(i.BrokenLinkReason));

        var publicMenus = await GetPublicMenusAsync();
        var group = Assert.Single(publicMenus.Header, i => i.Label == "Grup g1");
        Assert.Null(group.Href);
        var child = Assert.Single(group.Children);
        Assert.Equal("/kampanya", child.Href);
        var external = Assert.Single(publicMenus.Header, i => i.Label == "Dış Link");
        Assert.Equal("https://example.com", external.Href);
    }

    [Fact]
    public async Task ReplaceMenuItems_HidesEmptyGroupHeadingFromPublicResponse()
    {
        var accessToken = await LoginAsAdminAsync();
        var current = await GetMenuAsync(accessToken, "Header");

        var putResponse = await ReplaceMenuAsync(
            accessToken, "Header", current.RowVersion,
            [
                Group("g2"),
                Leaf("c3", "g2", new MenuItemLinkInput("InternalPath", null, null, "/gizli", null), isActive: false, label: "Gizli Çocuk"),
            ]);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        var publicMenus = await GetPublicMenusAsync();
        Assert.DoesNotContain(publicMenus.Header, i => i.Label == "Grup g2");

        var adminMenu = await GetMenuAsync(accessToken, "Header");
        Assert.Contains(adminMenu.Items, i => i.Translations.Any(t => t.Label == "Grup g2"));
    }

    [Fact]
    public async Task ReplaceMenuItems_WithStaleRowVersion_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var current = await GetMenuAsync(accessToken, "Footer");

        var firstPut = await ReplaceMenuAsync(accessToken, "Footer", current.RowVersion, [Leaf("f1", null, null)]);
        Assert.Equal(HttpStatusCode.NoContent, firstPut.StatusCode);

        var secondPut = await ReplaceMenuAsync(accessToken, "Footer", current.RowVersion, [Leaf("f2", null, null)]);

        Assert.Equal(HttpStatusCode.Conflict, secondPut.StatusCode);
    }

    [Fact]
    public async Task ReplaceMenuItems_OnUtilityMenu_WithNestedItem_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var current = await GetMenuAsync(accessToken, "Utility");

        var response = await ReplaceMenuAsync(accessToken, "Utility", current.RowVersion, [Group("u1"), Leaf("u2", "u1", null)]);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ReplaceMenuItems_WithInvalidLocation_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();

        var response = await ReplaceMenuAsync(accessToken, "not-a-real-location", [1, 2, 3, 4, 5, 6, 7, 8], []);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UnpublishingLinkedContent_RemovesMenuLinkFromPublicSite()
    {
        var accessToken = await LoginAsAdminAsync();
        var contentItemId = await CreateAndPublishNewsItemAsync(accessToken);
        var current = await GetMenuAsync(accessToken, "Header");

        var putResponse = await ReplaceMenuAsync(
            accessToken, "Header", current.RowVersion,
            [Leaf("n1", null, new MenuItemLinkInput("Content", contentItemId, null, null, null), label: "Haberim")]);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        var beforeUnpublish = await GetPublicMenusAsync();
        var linkedItem = Assert.Single(beforeUnpublish.Header, i => i.Label == "Haberim");
        Assert.NotNull(linkedItem.Href);

        var detail = await GetContentItemAsync(accessToken, contentItemId);
        var unpublishResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{contentItemId}/unpublish", accessToken,
            new UnpublishContentItemRequest(detail.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, unpublishResponse.StatusCode);

        var afterUnpublish = await GetPublicMenusAsync();
        Assert.DoesNotContain(afterUnpublish.Header, i => i.Label == "Haberim");
    }

    [Fact]
    public async Task PermanentlyDeletingLinkedContent_DeactivatesMenuItemAndClearsItsLink()
    {
        var accessToken = await LoginAsAdminAsync();
        var contentItemId = await CreateAndPublishNewsItemAsync(accessToken);
        var current = await GetMenuAsync(accessToken, "Header");

        var putResponse = await ReplaceMenuAsync(
            accessToken, "Header", current.RowVersion,
            [Leaf("d1", null, new MenuItemLinkInput("Content", contentItemId, null, null, null), label: "SilinecekHaber")]);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        // A published item must be unpublished before it can move to the trash (ContentItem.MoveToTrash).
        var beforeTrash = await GetContentItemAsync(accessToken, contentItemId);
        var unpublishResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{contentItemId}/unpublish", accessToken,
            new UnpublishContentItemRequest(beforeTrash.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, unpublishResponse.StatusCode);

        var detail = await GetContentItemAsync(accessToken, contentItemId);
        var trashResponse = await _client.SendAsync(Authorized(
            HttpMethod.Delete, $"/api/v1/admin/website/contents/{contentItemId}", accessToken,
            new DeleteContentItemRequest(detail.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, trashResponse.StatusCode);

        var permanentDeleteResponse = await _client.SendAsync(
            Authorized(HttpMethod.Delete, $"/api/v1/admin/website/trash/{contentItemId}", accessToken));
        Assert.Equal(HttpStatusCode.NoContent, permanentDeleteResponse.StatusCode);

        var adminMenu = await GetMenuAsync(accessToken, "Header");
        var deactivatedItem = adminMenu.Items.Single(i => i.Translations.Any(t => t.Label == "SilinecekHaber"));
        Assert.False(deactivatedItem.IsActive);
        Assert.Null(deactivatedItem.Link);
    }

    private sealed record PublicSiteMenusOnlyResponse(PublicMenusOnlyResponse Menus);

    private sealed record PublicMenusOnlyResponse(
        List<PublicMenuItemOnlyResponse> Header, List<PublicMenuItemOnlyResponse> Utility, List<PublicMenuItemOnlyResponse> Footer);

    private sealed record PublicMenuItemOnlyResponse(
        string Label, string? Href, bool OpenInNewTab, string? IconKey, List<PublicMenuItemOnlyResponse> Children);
}
