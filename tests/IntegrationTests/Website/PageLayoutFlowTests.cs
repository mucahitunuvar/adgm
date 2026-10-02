using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.CreateSlider;
using GenclikMerkezi.Modules.Website.Features.CreateVideo;
using GenclikMerkezi.Modules.Website.Features.DeleteContentItem;
using GenclikMerkezi.Modules.Website.Features.DiscardContentLayoutDraft;
using GenclikMerkezi.Modules.Website.Features.DiscardHomeLayoutDraft;
using GenclikMerkezi.Modules.Website.Features.GetBlockTypes;
using GenclikMerkezi.Modules.Website.Features.GetContentItemById;
using GenclikMerkezi.Modules.Website.Features.GetContentLayout;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.GetHomeLayout;
using GenclikMerkezi.Modules.Website.Features.PublishContentLayout;
using GenclikMerkezi.Modules.Website.Features.PublishHomeLayout;
using GenclikMerkezi.Modules.Website.Features.ReplaceContentDraftBlocks;
using GenclikMerkezi.Modules.Website.Features.ReplaceHomeDraftBlocks;
using GenclikMerkezi.Modules.Website.Features.UploadMediaAsset;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;

namespace GenclikMerkezi.IntegrationTests.Website;

// Faz 2 Görev 4. Shares the "one CustomWebApplicationFactory/one Sqlite database per class" caveat
// every other Website flow test class documents - the seeded Home PageLayout row is therefore shared
// across every test method here, so only HomeLayout_FullDraftPublishChangeDiscardLifecycle touches it;
// every other test uses a freshly created Content item's own layout to stay isolated.
public class PageLayoutFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions CamelCase = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PageLayoutFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<string> LoginAsNonAdminAsync()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email, password = "Sifre123", firstName = "Test", lastName = "User", role = "Candidate" });
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Sifre123" });
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

    private static JsonElement J(object value) => JsonSerializer.SerializeToElement(value, CamelCase);

    // --- Content types / content items ---

    private async Task<Guid> GetContentTypeIdByKeyAsync(string accessToken, string key)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/content-types", accessToken));
        var types = await response.Content.ReadFromJsonAsync<List<ContentTypeSummaryResponse>>();
        return types!.Single(t => t.Key == key).Id;
    }

    private async Task<Guid> CreateContentItemAsync(string accessToken, Guid contentTypeId, string title = "Başlık")
    {
        var emptySeo = new CreateContentItemSeoInput(null, null, null, null, null, null, null, false);
        var request = new CreateContentItemRequest(
            contentTypeId, null, 1, false, null, null, title, $"slug-{Guid.NewGuid():N}", null, null, emptySeo);
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, request));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreateContentItemResponse>();
        return created!.Id;
    }

    private async Task<ContentItemDetailResponse> GetContentItemAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{id}", accessToken));
        return (await response.Content.ReadFromJsonAsync<ContentItemDetailResponse>())!;
    }

    private async Task MoveToTrashAsync(string accessToken, Guid contentItemId)
    {
        var item = await GetContentItemAsync(accessToken, contentItemId);
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/admin/website/contents/{contentItemId}")
        {
            Content = JsonContent.Create(new DeleteContentItemRequest(item.RowVersion)),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<HttpResponseMessage> PermanentlyDeleteContentItemAsync(string accessToken, Guid contentItemId) =>
        await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/trash/{contentItemId}", accessToken));

    // --- Sliders / videos / media ---

    private async Task<Guid> CreateSliderAsync(string accessToken)
    {
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/sliders", accessToken,
            new CreateSliderRequest($"slider-{Guid.NewGuid():N}"[..20], "Test Slider")));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreateSliderResponse>();
        return created!.Id;
    }

    private async Task<Guid> CreateVideoAsync(string accessToken)
    {
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/videos", accessToken,
            new CreateVideoRequest("https://youtu.be/dQw4w9WgXcQ", null, 1, "Test Video", null)));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreateVideoResponse>();
        return created!.Id;
    }

    private static byte[] CreateJpeg(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.CornflowerBlue);
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        return data.ToArray();
    }

    private async Task<Guid> UploadImageAsync(string accessToken)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(CreateJpeg(400, 300));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "block.jpg");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/website/media") { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var uploaded = await response.Content.ReadFromJsonAsync<UploadMediaAssetResponse>();
        return uploaded!.Id;
    }

    // --- Home layout ---

    private async Task<GetHomeLayoutResponse> GetHomeLayoutAsync(string accessToken)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/layouts/home", accessToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<GetHomeLayoutResponse>())!;
    }

    private async Task<HttpResponseMessage> ReplaceHomeDraftAsync(string accessToken, byte[] rowVersion, IReadOnlyList<LayoutBlockInput> blocks) =>
        await _client.SendAsync(Authorized(
            HttpMethod.Put, "/api/v1/admin/website/layouts/home/draft", accessToken, new ReplaceHomeDraftBlocksRequest(rowVersion, blocks)));

    private async Task<HttpResponseMessage> PublishHomeAsync(string accessToken, byte[] rowVersion) =>
        await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/layouts/home/publish", accessToken, new PublishHomeLayoutRequest(rowVersion)));

    private async Task<HttpResponseMessage> DiscardHomeDraftAsync(string accessToken, byte[] rowVersion) =>
        await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/layouts/home/discard-draft", accessToken, new DiscardHomeLayoutDraftRequest(rowVersion)));

    // --- Content layout ---

    private async Task<GetContentLayoutResponse> GetContentLayoutAsync(string accessToken, Guid contentItemId)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/layouts/content/{contentItemId}", accessToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<GetContentLayoutResponse>())!;
    }

    private async Task<HttpResponseMessage> ReplaceContentDraftAsync(
        string accessToken, Guid contentItemId, byte[] rowVersion, IReadOnlyList<LayoutBlockInput> blocks) =>
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/layouts/content/{contentItemId}/draft", accessToken,
            new ReplaceContentDraftBlocksRequest(rowVersion, blocks)));

    private async Task<HttpResponseMessage> PublishContentAsync(string accessToken, Guid contentItemId, byte[] rowVersion) =>
        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/layouts/content/{contentItemId}/publish", accessToken,
            new PublishContentLayoutRequest(rowVersion)));

    private async Task<HttpResponseMessage> DiscardContentDraftAsync(string accessToken, Guid contentItemId, byte[] rowVersion) =>
        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/layouts/content/{contentItemId}/discard-draft", accessToken,
            new DiscardContentLayoutDraftRequest(rowVersion)));

    // --- Block builders ---

    private static LayoutBlockInput HeroSliderBlock(Guid sliderId, int sortOrder = 1) =>
        new("hero-slider", sortOrder, true, J(new { sliderId }));

    private static LayoutBlockInput QuickLinksBlock(int sortOrder = 1) =>
        new(
            "quick-links", sortOrder, true,
            J(new
            {
                items = new object[]
                {
                    new
                    {
                        iconKey = "star",
                        link = new
                        {
                            kind = "ExternalUrl", contentItemId = (Guid?)null, contentTypeId = (Guid?)null, internalPath = (string?)null,
                            externalUrl = "https://example.com",
                        },
                    },
                },
            }),
            new Dictionary<string, JsonElement>
            {
                ["tr"] = J(new { items = new object[] { new { label = "Bağlantı", description = (string?)null } } }),
            });

    private static LayoutBlockInput RichTextBlock(int sortOrder = 1, string body = "<p>metin</p>") =>
        new("rich-text", sortOrder, true, J(new { }), new Dictionary<string, JsonElement> { ["tr"] = J(new { body }) });

    private static LayoutBlockInput ImageTextBlock(Guid imageMediaId, int sortOrder = 1) =>
        new(
            "image-text", sortOrder, true,
            J(new { imageMediaId, imagePosition = "left", link = (object?)null }),
            new Dictionary<string, JsonElement>
            {
                ["tr"] = J(new { title = "Başlık", body = "<p>x</p>", linkLabel = (string?)null }),
            });

    private static LayoutBlockInput VideoFeatureBlock(Guid videoId, int sortOrder = 1) =>
        new(
            "video-feature", sortOrder, true, J(new { videoId }),
            new Dictionary<string, JsonElement>
            {
                ["tr"] = J(new { eyebrow = (string?)null, title = "Video", text = (string?)null, link = (object?)null, linkLabel = (string?)null }),
            });

    private static LayoutBlockInput JobListBlock(int sortOrder = 1) =>
        new(
            "job-list", sortOrder, true, J(new { count = 3 }),
            new Dictionary<string, JsonElement> { ["tr"] = J(new { title = "İlanlar", moreLabel = (string?)null }) });

    // --- 1. Home: draft -> publish -> change -> discard ---

    [Fact]
    public async Task HomeLayout_FullDraftPublishChangeDiscardLifecycle()
    {
        var accessToken = await LoginAsAdminAsync();
        var sliderId = await CreateSliderAsync(accessToken);

        var initial = await GetHomeLayoutAsync(accessToken);
        Assert.Empty(initial.DraftBlocks);
        Assert.Empty(initial.PublishedBlocks);
        Assert.False(initial.HasUnpublishedChanges);

        var putResponse = await ReplaceHomeDraftAsync(accessToken, initial.RowVersion, [QuickLinksBlock(1), HeroSliderBlock(sliderId, 2)]);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        var afterPut = await GetHomeLayoutAsync(accessToken);
        Assert.Equal(2, afterPut.DraftBlocks.Count);
        Assert.True(afterPut.HasUnpublishedChanges);
        Assert.Empty(afterPut.PublishedBlocks);

        // The settings/texts JSON must round-trip exactly as sent, including the nested quick-links array.
        var quickLinks = afterPut.DraftBlocks.Single(b => b.BlockTypeKey == "quick-links");
        Assert.Equal("star", quickLinks.Settings.GetProperty("items")[0].GetProperty("iconKey").GetString());
        Assert.Equal("Bağlantı", quickLinks.Texts["tr"].GetProperty("items")[0].GetProperty("label").GetString());
        var hero = afterPut.DraftBlocks.Single(b => b.BlockTypeKey == "hero-slider");
        Assert.Equal(sliderId, hero.Settings.GetProperty("sliderId").GetGuid());

        var publishResponse = await PublishHomeAsync(accessToken, afterPut.RowVersion);
        Assert.Equal(HttpStatusCode.NoContent, publishResponse.StatusCode);

        var afterPublish = await GetHomeLayoutAsync(accessToken);
        Assert.False(afterPublish.HasUnpublishedChanges);
        Assert.Equal(2, afterPublish.PublishedBlocks.Count);
        Assert.NotNull(afterPublish.PublishedAtUtc);

        var changeResponse = await ReplaceHomeDraftAsync(accessToken, afterPublish.RowVersion, [HeroSliderBlock(sliderId)]);
        Assert.Equal(HttpStatusCode.NoContent, changeResponse.StatusCode);

        var afterChange = await GetHomeLayoutAsync(accessToken);
        Assert.True(afterChange.HasUnpublishedChanges);
        Assert.Single(afterChange.DraftBlocks);
        Assert.Equal(2, afterChange.PublishedBlocks.Count);

        var discardResponse = await DiscardHomeDraftAsync(accessToken, afterChange.RowVersion);
        Assert.Equal(HttpStatusCode.NoContent, discardResponse.StatusCode);

        var afterDiscard = await GetHomeLayoutAsync(accessToken);
        Assert.False(afterDiscard.HasUnpublishedChanges);
        Assert.Equal(2, afterDiscard.DraftBlocks.Count);
    }

    // --- 2. Content: first save creates the layout; SupportsBlockLayout/trash gating ---

    [Fact]
    public async Task ContentLayout_FirstSave_CreatesLayoutForContentItem()
    {
        var accessToken = await LoginAsAdminAsync();
        var itemId = await CreateContentItemAsync(accessToken, await GetContentTypeIdByKeyAsync(accessToken, "page"));

        var initial = await GetContentLayoutAsync(accessToken, itemId);
        Assert.Empty(initial.RowVersion);
        Assert.Empty(initial.DraftBlocks);

        var putResponse = await ReplaceContentDraftAsync(accessToken, itemId, initial.RowVersion, [RichTextBlock()]);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        var afterPut = await GetContentLayoutAsync(accessToken, itemId);
        Assert.Single(afterPut.DraftBlocks);
        Assert.NotEmpty(afterPut.RowVersion);
    }

    [Fact]
    public async Task ContentLayout_ForTypeWithoutSupportsBlockLayout_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var itemId = await CreateContentItemAsync(accessToken, await GetContentTypeIdByKeyAsync(accessToken, "news"));

        var response = await ReplaceContentDraftAsync(accessToken, itemId, [], [RichTextBlock()]);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ContentLayout_ForTrashedItem_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var itemId = await CreateContentItemAsync(accessToken, await GetContentTypeIdByKeyAsync(accessToken, "page"));
        await MoveToTrashAsync(accessToken, itemId);

        var response = await ReplaceContentDraftAsync(accessToken, itemId, [], [RichTextBlock()]);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- 3. Validation ---

    [Fact]
    public async Task ReplaceContentDraft_WithUnknownSettingsField_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var itemId = await CreateContentItemAsync(accessToken, await GetContentTypeIdByKeyAsync(accessToken, "page"));
        var sliderId = await CreateSliderAsync(accessToken);
        var badBlock = new LayoutBlockInput("hero-slider", 1, true, J(new { sliderId, extra = 1 }));

        var response = await ReplaceContentDraftAsync(accessToken, itemId, [], [badBlock]);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ReplaceContentDraft_WithUndefinedBlockType_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var itemId = await CreateContentItemAsync(accessToken, await GetContentTypeIdByKeyAsync(accessToken, "page"));
        var badBlock = new LayoutBlockInput("does-not-exist", 1, true, J(new { }));

        var response = await ReplaceContentDraftAsync(accessToken, itemId, [], [badBlock]);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ReplaceContentDraft_WithJobListBlock_ReturnsBadRequest()
    {
        // §4.2 "job-list yalnızca Home hedefinde kullanılabilir".
        var accessToken = await LoginAsAdminAsync();
        var itemId = await CreateContentItemAsync(accessToken, await GetContentTypeIdByKeyAsync(accessToken, "page"));

        var response = await ReplaceContentDraftAsync(accessToken, itemId, [], [JobListBlock()]);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ReplaceContentDraft_WithNonExistentMediaId_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var itemId = await CreateContentItemAsync(accessToken, await GetContentTypeIdByKeyAsync(accessToken, "page"));

        var response = await ReplaceContentDraftAsync(accessToken, itemId, [], [ImageTextBlock(Guid.NewGuid())]);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- 4. Concurrency ---

    [Fact]
    public async Task ReplaceContentDraft_WithStaleRowVersion_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var itemId = await CreateContentItemAsync(accessToken, await GetContentTypeIdByKeyAsync(accessToken, "page"));
        var initial = await GetContentLayoutAsync(accessToken, itemId);

        var firstPut = await ReplaceContentDraftAsync(accessToken, itemId, initial.RowVersion, [RichTextBlock()]);
        Assert.Equal(HttpStatusCode.NoContent, firstPut.StatusCode);
        var staleRowVersion = (await GetContentLayoutAsync(accessToken, itemId)).RowVersion;

        var secondPut = await ReplaceContentDraftAsync(accessToken, itemId, staleRowVersion, [RichTextBlock(body: "<p>v2</p>")]);
        Assert.Equal(HttpStatusCode.NoContent, secondPut.StatusCode);

        var thirdPut = await ReplaceContentDraftAsync(accessToken, itemId, staleRowVersion, [RichTextBlock(body: "<p>v3</p>")]);

        Assert.Equal(HttpStatusCode.Conflict, thirdPut.StatusCode);
    }

    // --- 5. Authorization ---

    [Fact]
    public async Task GetBlockTypes_AsNonAdmin_ReturnsForbidden()
    {
        var accessToken = await LoginAsNonAdminAsync();

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/block-types", accessToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ReplaceHomeDraft_AsNonAdmin_ReturnsForbidden()
    {
        var accessToken = await LoginAsNonAdminAsync();

        var response = await ReplaceHomeDraftAsync(accessToken, [], []);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // --- 6. Usage protection ---

    [Fact]
    public async Task DeleteMediaAsset_UsedByPublishedImageTextBlock_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var imageId = await UploadImageAsync(accessToken);
        var itemId = await CreateContentItemAsync(accessToken, await GetContentTypeIdByKeyAsync(accessToken, "page"));
        await ReplaceContentDraftAsync(accessToken, itemId, [], [ImageTextBlock(imageId)]);
        var afterPut = await GetContentLayoutAsync(accessToken, itemId);
        var publishResponse = await PublishContentAsync(accessToken, itemId, afterPut.RowVersion);
        Assert.Equal(HttpStatusCode.NoContent, publishResponse.StatusCode);

        var response = await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/media/{imageId}", accessToken));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMediaAsset_UsedOnlyByDraftImageTextBlock_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var imageId = await UploadImageAsync(accessToken);
        var itemId = await CreateContentItemAsync(accessToken, await GetContentTypeIdByKeyAsync(accessToken, "page"));
        var putResponse = await ReplaceContentDraftAsync(accessToken, itemId, [], [ImageTextBlock(imageId)]);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        var response = await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/media/{imageId}", accessToken));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteSlider_UsedByPublishedHeroSliderBlock_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var sliderId = await CreateSliderAsync(accessToken);
        var itemId = await CreateContentItemAsync(accessToken, await GetContentTypeIdByKeyAsync(accessToken, "page"));
        await ReplaceContentDraftAsync(accessToken, itemId, [], [HeroSliderBlock(sliderId)]);
        var afterPut = await GetContentLayoutAsync(accessToken, itemId);
        var publishResponse = await PublishContentAsync(accessToken, itemId, afterPut.RowVersion);
        Assert.Equal(HttpStatusCode.NoContent, publishResponse.StatusCode);

        var response = await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/sliders/{sliderId}", accessToken));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteVideo_UsedByPublishedVideoFeatureBlock_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var videoId = await CreateVideoAsync(accessToken);
        var itemId = await CreateContentItemAsync(accessToken, await GetContentTypeIdByKeyAsync(accessToken, "page"));
        await ReplaceContentDraftAsync(accessToken, itemId, [], [VideoFeatureBlock(videoId)]);
        var afterPut = await GetContentLayoutAsync(accessToken, itemId);
        var publishResponse = await PublishContentAsync(accessToken, itemId, afterPut.RowVersion);
        Assert.Equal(HttpStatusCode.NoContent, publishResponse.StatusCode);

        var response = await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/videos/{videoId}", accessToken));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // --- 7. Permanent deletion cascade ---

    [Fact]
    public async Task PermanentlyDeleteContentItem_AlsoRemovesItsPageLayout()
    {
        var accessToken = await LoginAsAdminAsync();
        var itemId = await CreateContentItemAsync(accessToken, await GetContentTypeIdByKeyAsync(accessToken, "page"));
        var putResponse = await ReplaceContentDraftAsync(accessToken, itemId, [], [RichTextBlock()]);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IPageLayoutRepository>();
            Assert.NotNull(await repository.GetByContentItemIdAsync(itemId));
        }

        await MoveToTrashAsync(accessToken, itemId);
        var permanentDeleteResponse = await PermanentlyDeleteContentItemAsync(accessToken, itemId);
        Assert.Equal(HttpStatusCode.NoContent, permanentDeleteResponse.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IPageLayoutRepository>();
            Assert.Null(await repository.GetByContentItemIdAsync(itemId));
        }
    }

    // --- 8. Block-types catalog ---

    [Fact]
    public async Task GetBlockTypes_ReturnsAllFifteenTypesWithFieldDescriptions()
    {
        var accessToken = await LoginAsAdminAsync();

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/block-types", accessToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var types = await response.Content.ReadFromJsonAsync<List<BlockTypeResponse>>();

        Assert.Equal(15, types!.Count);
        var jobList = types.Single(t => t.Key == "job-list");
        Assert.Equal(["Home"], jobList.AllowedTargets);
        Assert.Contains(jobList.SettingsFields, f => f.Name == "count" && f.Type == "int");
    }
}
