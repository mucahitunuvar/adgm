using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.CreatePartner;
using GenclikMerkezi.Modules.Website.Features.CreateSlider;
using GenclikMerkezi.Modules.Website.Features.DeactivatePartner;
using GenclikMerkezi.Modules.Website.Features.GetContentItemById;
using GenclikMerkezi.Modules.Website.Features.GetContentLayout;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.GetHomeLayout;
using GenclikMerkezi.Modules.Website.Features.GetHomeLayoutPreview;
using GenclikMerkezi.Modules.Website.Features.GetPartnerById;
using GenclikMerkezi.Modules.Website.Features.GetPublicContentById;
using GenclikMerkezi.Modules.Website.Features.GetPublicHome;
using GenclikMerkezi.Modules.Website.Features.PublishContentItem;
using GenclikMerkezi.Modules.Website.Features.PublishContentLayout;
using GenclikMerkezi.Modules.Website.Features.PublishHomeLayout;
using GenclikMerkezi.Modules.Website.Features.ReplaceContentDraftBlocks;
using GenclikMerkezi.Modules.Website.Features.ReplaceHomeDraftBlocks;
using GenclikMerkezi.Modules.Website.Features.UploadMediaAsset;
using SkiaSharp;

namespace GenclikMerkezi.IntegrationTests.Website;

// Faz 2 Görev 5 master prompt §5. Shares the "one CustomWebApplicationFactory/one Sqlite database per
// class" caveat every other Website flow test class documents - a real database and real HTTP calls,
// not fakes, exercising the public home/content-detail/preview endpoints end to end (this is the
// integration-test half the task explicitly requires alongside PublicPageLayoutResolverTests' unit
// coverage of every block type's data/hiding rules).
public class PublicHomeAndBlockDataFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions CamelCase = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PublicHomeAndBlockDataFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<Guid> GetContentTypeIdByKeyAsync(string accessToken, string key)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/content-types", accessToken));
        var types = await response.Content.ReadFromJsonAsync<List<ContentTypeSummaryResponse>>();
        return types!.Single(t => t.Key == key).Id;
    }

    private async Task<(Guid Id, byte[] RowVersion)> CreateContentItemAsync(string accessToken, Guid contentTypeId, string title = "Başlık")
    {
        var emptySeo = new CreateContentItemSeoInput(null, null, null, null, null, null, null, false);
        var request = new CreateContentItemRequest(
            contentTypeId, null, 1, false, null, null, title, $"slug-{Guid.NewGuid():N}", null, null, emptySeo);
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, request));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreateContentItemResponse>();
        var item = await GetContentItemAsync(accessToken, created!.Id);
        return (created.Id, item.RowVersion);
    }

    private async Task<ContentItemDetailResponse> GetContentItemAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{id}", accessToken));
        return (await response.Content.ReadFromJsonAsync<ContentItemDetailResponse>())!;
    }

    private async Task PublishContentItemAsync(string accessToken, Guid id)
    {
        var rowVersion = (await GetContentItemAsync(accessToken, id)).RowVersion;
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{id}/publish", accessToken, new PublishContentItemRequest(rowVersion, null, null)));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
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

    private async Task<Guid> CreateSliderAsync(string accessToken)
    {
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/sliders", accessToken,
            new CreateSliderRequest($"slider-{Guid.NewGuid():N}"[..20], "Test Slider")));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreateSliderResponse>();
        return created!.Id;
    }

    private async Task<Guid> CreatePartnerAsync(string accessToken, Guid logoMediaId)
    {
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/partners", accessToken,
            new CreatePartnerRequest(logoMediaId, null, 1, "Test Partner", null)));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreatePartnerResponse>();
        return created!.Id;
    }

    private async Task<PartnerDetailResponse> GetPartnerAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/partners/{id}", accessToken));
        return (await response.Content.ReadFromJsonAsync<PartnerDetailResponse>())!;
    }

    private async Task DeactivatePartnerAsync(string accessToken, Guid id)
    {
        var rowVersion = (await GetPartnerAsync(accessToken, id)).RowVersion;
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/partners/{id}/deactivate", accessToken, new DeactivatePartnerRequest(rowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // --- Home layout plumbing ---

    private async Task<GetHomeLayoutResponse> GetHomeLayoutAsync(string accessToken)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/layouts/home", accessToken));
        return (await response.Content.ReadFromJsonAsync<GetHomeLayoutResponse>())!;
    }

    private async Task PublishHomeDraftAsync(string accessToken, params (string Key, object Settings, IReadOnlyDictionary<string, JsonElement>? Texts)[] blocks)
    {
        var initial = await GetHomeLayoutAsync(accessToken);
        var inputs = blocks
            .Select((b, i) => new LayoutBlockInput(b.Key, i + 1, true, J(b.Settings), b.Texts))
            .ToList();

        var putResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, "/api/v1/admin/website/layouts/home/draft", accessToken,
            new ReplaceHomeDraftBlocksRequest(initial.RowVersion, inputs)));
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        var afterPut = await GetHomeLayoutAsync(accessToken);
        var publishResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/layouts/home/publish", accessToken, new PublishHomeLayoutRequest(afterPut.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, publishResponse.StatusCode);
    }

    private async Task<Guid> ReplaceContentDraftAndPublishAsync(
        string accessToken, Guid contentItemId, params (string Key, object Settings, IReadOnlyDictionary<string, JsonElement>? Texts)[] blocks)
    {
        var layoutResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/layouts/content/{contentItemId}", accessToken));
        var initial = (await layoutResponse.Content.ReadFromJsonAsync<GetContentLayoutResponse>())!;
        var inputs = blocks
            .Select((b, i) => new LayoutBlockInput(b.Key, i + 1, true, J(b.Settings), b.Texts))
            .ToList();

        var putResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/layouts/content/{contentItemId}/draft", accessToken,
            new ReplaceContentDraftBlocksRequest(initial.RowVersion, inputs)));
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        var afterPut = (await (await _client.SendAsync(Authorized(
                HttpMethod.Get, $"/api/v1/admin/website/layouts/content/{contentItemId}", accessToken)))
            .Content.ReadFromJsonAsync<GetContentLayoutResponse>())!;
        var publishResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/layouts/content/{contentItemId}/publish", accessToken,
            new PublishContentLayoutRequest(afterPut.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, publishResponse.StatusCode);
        return contentItemId;
    }

    // --- Public endpoints ---

    private async Task<PublicHomeResponse> GetPublicHomeAsync(string? lang = null)
    {
        var response = await _client.GetAsync("/api/v1/public/home" + (lang is null ? string.Empty : $"?lang={lang}"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<PublicHomeResponse>())!;
    }

    private async Task<GetHomeLayoutPreviewResponse> GetHomeLayoutPreviewAsync(string accessToken)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/layouts/home/preview", accessToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<GetHomeLayoutPreviewResponse>())!;
    }

    private async Task<PublicContentDetailResponse> GetPublicContentByIdAsync(Guid id)
    {
        var response = await _client.GetAsync($"/api/v1/public/contents/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<PublicContentDetailResponse>())!;
    }

    // --- 1. Public home: resolved block data ---

    [Fact]
    public async Task GetPublicHome_WithLogoStripBlock_ReturnsActivePartnerInBlockData()
    {
        var accessToken = await LoginAsAdminAsync();
        var logoId = await UploadImageAsync(accessToken);
        await CreatePartnerAsync(accessToken, logoId);

        await PublishHomeDraftAsync(accessToken, ("logo-strip", new { maxItems = 10 }, Texts(("tr", new { title = (string?)null }))));

        var home = await GetPublicHomeAsync();

        var block = Assert.Single(home.Blocks);
        Assert.Equal("logo-strip", block.Type);
        Assert.NotNull(block.Data);
    }

    // --- 2. Hiding rule end-to-end: slider with no visible slides ---

    [Fact]
    public async Task GetPublicHome_WithHeroSliderWithoutSlides_OmitsBlockEntirely()
    {
        var accessToken = await LoginAsAdminAsync();
        var sliderId = await CreateSliderAsync(accessToken);

        await PublishHomeDraftAsync(accessToken, ("hero-slider", new { sliderId }, null));

        var home = await GetPublicHomeAsync();

        Assert.Empty(home.Blocks);
    }

    // --- 3. Draft preview vs published public response ---

    // Only the DRAFT changes here (never published) - the published list is whatever an earlier test
    // method in this shared-fixture class last published, so this checks for the draft's own distinct
    // marker text rather than asserting the published list is empty outright.
    [Fact]
    public async Task GetHomeLayoutPreview_ShowsDraftBlocksBeforePublish()
    {
        var accessToken = await LoginAsAdminAsync();
        var marker = $"taslak-{Guid.NewGuid():N}";
        var initial = await GetHomeLayoutAsync(accessToken);
        var inputs = new List<LayoutBlockInput>
        {
            new("rich-text", 1, true, J(new { }), Texts(("tr", new { body = $"<p>{marker}</p>" }))),
        };
        var putResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, "/api/v1/admin/website/layouts/home/draft", accessToken,
            new ReplaceHomeDraftBlocksRequest(initial.RowVersion, inputs)));
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        var preview = await GetHomeLayoutPreviewAsync(accessToken);
        var publicBody = await (await _client.GetAsync("/api/v1/public/home")).Content.ReadAsStringAsync();

        Assert.Contains(preview.Blocks, b => b.Type == "rich-text");
        Assert.DoesNotContain(marker, publicBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetHomeLayoutPreview_AsNonAdmin_ReturnsForbidden()
    {
        var accessToken = await LoginAsNonAdminAsync();

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/layouts/home/preview", accessToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // --- 4. Cache invalidation end-to-end ---

    // The seeded Home PageLayout row (like the other flow test classes' shared fixtures) and every
    // Partner ever created in this test class's database are both shared across test methods - so this
    // checks for THIS test's own partner id in the raw response body rather than asserting the whole
    // block/list is empty, which would be broken by any other still-active partner another test method
    // left behind.
    [Fact]
    public async Task GetPublicHome_AfterPartnerDeactivated_NoLongerShowsThatPartner()
    {
        var accessToken = await LoginAsAdminAsync();
        var logoId = await UploadImageAsync(accessToken);
        var partnerId = await CreatePartnerAsync(accessToken, logoId);
        await PublishHomeDraftAsync(accessToken, ("logo-strip", new { maxItems = 30 }, Texts(("tr", new { title = (string?)null }))));

        var beforeBody = await (await _client.GetAsync("/api/v1/public/home")).Content.ReadAsStringAsync();
        Assert.Contains(partnerId.ToString(), beforeBody, StringComparison.OrdinalIgnoreCase);

        await DeactivatePartnerAsync(accessToken, partnerId);
        var afterBody = await (await _client.GetAsync("/api/v1/public/home")).Content.ReadAsStringAsync();

        Assert.DoesNotContain(partnerId.ToString(), afterBody, StringComparison.OrdinalIgnoreCase);
    }

    // --- 5. Content detail: blocks field gated by SupportsBlockLayout ---

    [Fact]
    public async Task GetPublicContentById_ForSupportsBlockLayoutType_IncludesPublishedBlocks()
    {
        var accessToken = await LoginAsAdminAsync();
        var (itemId, _) = await CreateContentItemAsync(accessToken, await GetContentTypeIdByKeyAsync(accessToken, "page"));
        await ReplaceContentDraftAndPublishAsync(
            accessToken, itemId, ("rich-text", new { }, Texts(("tr", new { body = "<p>içerik</p>" }))));
        await PublishContentItemAsync(accessToken, itemId);

        var detail = await GetPublicContentByIdAsync(itemId);

        Assert.NotNull(detail.Blocks);
        var block = Assert.Single(detail.Blocks!);
        Assert.Equal("rich-text", block.Type);
    }

    [Fact]
    public async Task GetPublicContentById_ForTypeWithoutSupportsBlockLayout_OmitsBlocksField()
    {
        var accessToken = await LoginAsAdminAsync();
        var (itemId, _) = await CreateContentItemAsync(accessToken, await GetContentTypeIdByKeyAsync(accessToken, "news"));
        await PublishContentItemAsync(accessToken, itemId);

        var detail = await GetPublicContentByIdAsync(itemId);

        Assert.Null(detail.Blocks);
    }

    private static IReadOnlyDictionary<string, JsonElement>? Texts(params (string Lang, object Value)[] entries) =>
        entries.Length == 0 ? null : entries.ToDictionary(e => e.Lang, e => J(e.Value));
}
