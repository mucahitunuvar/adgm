using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.ActivateVideo;
using GenclikMerkezi.Modules.Website.Features.CreateSiteLanguage;
using GenclikMerkezi.Modules.Website.Features.CreateVideo;
using GenclikMerkezi.Modules.Website.Features.DeactivateVideo;
using GenclikMerkezi.Modules.Website.Features.DeleteVideoTranslation;
using GenclikMerkezi.Modules.Website.Features.GetPublicVideos;
using GenclikMerkezi.Modules.Website.Features.GetVideoById;
using GenclikMerkezi.Modules.Website.Features.GetVideos;
using GenclikMerkezi.Modules.Website.Features.UpdateVideo;
using GenclikMerkezi.Modules.Website.Features.UpdateVideoTranslation;
using GenclikMerkezi.Modules.Website.Features.UploadMediaAsset;
using GenclikMerkezi.SharedKernel.Results;
using SkiaSharp;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §5 (Faz 1b Görev 2). Shares the "one CustomWebApplicationFactory/one Sqlite database per
// class" caveat every other Website flow test class documents.
public class VideoFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private const string ValidUrl = "https://youtu.be/dQw4w9WgXcQ";

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public VideoFlowTests(CustomWebApplicationFactory factory)
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
        // 2000x1000 - large enough (ADR-024 §6: 400/800/1600px) to generate all three variants; a
        // smaller original silently skips whichever variants it can't upscale into.
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(CreateJpeg(2000, 1000));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "cover.jpg");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/website/media") { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var uploaded = await response.Content.ReadFromJsonAsync<UploadMediaAssetResponse>();
        return uploaded!.Id;
    }

    private async Task<Guid> UploadDocumentAsync(string accessToken)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent("%PDF-1.4 not a real pdf but only the extension/content-type matter here"u8.ToArray());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", "document.pdf");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/website/media") { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var uploaded = await response.Content.ReadFromJsonAsync<UploadMediaAssetResponse>();
        return uploaded!.Id;
    }

    private async Task<CreateVideoResponse> CreateVideoAsync(
        string accessToken, string? url = null, Guid? coverImageMediaId = null, int sortOrder = 1, string title = "Tanıtım Videosu")
    {
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/videos", accessToken,
            new CreateVideoRequest(url ?? ValidUrl, coverImageMediaId, sortOrder, title, null)));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CreateVideoResponse>())!;
    }

    private async Task<VideoDetailResponse> GetVideoAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/videos/{id}", accessToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<VideoDetailResponse>())!;
    }

    [Fact]
    public async Task CreateVideo_WithValidUrl_ExposesEmbedAndThumbnailUrls()
    {
        var accessToken = await LoginAsAdminAsync();

        var created = await CreateVideoAsync(accessToken);
        var detail = await GetVideoAsync(accessToken, created.Id);

        Assert.Equal("dQw4w9WgXcQ", detail.YouTubeVideoId);
        Assert.Equal("https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ", detail.EmbedUrl);
        Assert.Equal("https://i.ytimg.com/vi/dQw4w9WgXcQ/hqdefault.jpg", detail.YouTubeThumbnailUrl);
        Assert.Null(detail.CoverImage);
        Assert.True(detail.IsActive);
        Assert.Single(detail.Translations, t => t.LanguageCode == "tr" && t.Title == "Tanıtım Videosu");
    }

    [Fact]
    public async Task CreateVideo_WithUnrecognizedUrl_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/videos", accessToken,
            new CreateVideoRequest("https://vimeo.com/12345", null, 1, "Başlık", null)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateVideo_WithImageCoverImage_ReturnsVariantUrls()
    {
        var accessToken = await LoginAsAdminAsync();
        var coverImageId = await UploadImageAsync(accessToken);

        var created = await CreateVideoAsync(accessToken, coverImageMediaId: coverImageId);
        var detail = await GetVideoAsync(accessToken, created.Id);

        Assert.NotNull(detail.CoverImage);
        Assert.NotNull(detail.CoverImage!.Small);
        Assert.NotNull(detail.CoverImage.Medium);
        Assert.NotNull(detail.CoverImage.Large);
        Assert.NotEmpty(detail.CoverImage.Original);
    }

    [Fact]
    public async Task CreateVideo_WithDocumentAsCoverImage_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var documentId = await UploadDocumentAsync(accessToken);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/videos", accessToken,
            new CreateVideoRequest(ValidUrl, documentId, 1, "Başlık", null)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMediaAsset_WhenUsedAsVideoCoverImage_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var coverImageId = await UploadImageAsync(accessToken);
        await CreateVideoAsync(accessToken, coverImageMediaId: coverImageId);

        var response = await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/media/{coverImageId}", accessToken));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateVideo_WithStaleRowVersion_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateVideoAsync(accessToken);
        var staleRowVersion = (await GetVideoAsync(accessToken, created.Id)).RowVersion;

        // A first update succeeds and rotates the row version...
        var firstUpdate = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/videos/{created.Id}", accessToken,
            new UpdateVideoRequest(staleRowVersion, ValidUrl, null, 2)));
        Assert.Equal(HttpStatusCode.NoContent, firstUpdate.StatusCode);

        // ...so retrying with the now-stale row version must conflict.
        var secondUpdate = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/videos/{created.Id}", accessToken,
            new UpdateVideoRequest(staleRowVersion, ValidUrl, null, 3)));

        Assert.Equal(HttpStatusCode.Conflict, secondUpdate.StatusCode);
    }

    [Fact]
    public async Task AddUpdateAndDeleteTranslation_FullLifecycle()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateVideoAsync(accessToken);
        var rowVersion = (await GetVideoAsync(accessToken, created.Id)).RowVersion;

        var addResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/videos/{created.Id}/translations/en", accessToken,
            new UpdateVideoTranslationRequest(rowVersion, "Promo Video", "Description")));
        Assert.Equal(HttpStatusCode.NoContent, addResponse.StatusCode);

        var afterAdd = await GetVideoAsync(accessToken, created.Id);
        Assert.Equal(2, afterAdd.Translations.Count);
        Assert.Single(afterAdd.Translations, t => t.LanguageCode == "en" && t.Title == "Promo Video");

        var deleteResponse = await _client.SendAsync(Authorized(
            HttpMethod.Delete, $"/api/v1/admin/website/videos/{created.Id}/translations/en", accessToken,
            new DeleteVideoTranslationRequest(afterAdd.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDelete = await GetVideoAsync(accessToken, created.Id);
        Assert.Single(afterDelete.Translations);
    }

    [Fact]
    public async Task DeleteTranslation_ForDefaultLanguage_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateVideoAsync(accessToken);
        var rowVersion = (await GetVideoAsync(accessToken, created.Id)).RowVersion;

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Delete, $"/api/v1/admin/website/videos/{created.Id}/translations/tr", accessToken,
            new DeleteVideoTranslationRequest(rowVersion)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetVideos_FiltersByIsActiveAndSearch()
    {
        var accessToken = await LoginAsAdminAsync();
        var uniqueTitle = $"Benzersiz-{Guid.NewGuid():N}";
        var created = await CreateVideoAsync(accessToken, title: uniqueTitle);
        var rowVersion = (await GetVideoAsync(accessToken, created.Id)).RowVersion;
        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/videos/{created.Id}/deactivate", accessToken,
            new DeactivateVideoRequest(rowVersion)));

        var searchResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, $"/api/v1/admin/website/videos?search={uniqueTitle}", accessToken));
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<PagedResult<VideoSummaryResponse>>();
        Assert.Single(searchResult!.Items, i => i.Id == created.Id);

        var activeOnlyResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, $"/api/v1/admin/website/videos?search={uniqueTitle}&isActive=true", accessToken));
        var activeOnlyResult = await activeOnlyResponse.Content.ReadFromJsonAsync<PagedResult<VideoSummaryResponse>>();
        Assert.Empty(activeOnlyResult!.Items);
    }

    [Fact]
    public async Task PublicVideos_OnlyReturnsActiveVideosWithATranslationInTheRequestedLanguage()
    {
        var accessToken = await LoginAsAdminAsync();

        var active = await CreateVideoAsync(accessToken, title: $"Aktif-{Guid.NewGuid():N}");

        var inactive = await CreateVideoAsync(accessToken, title: $"Pasif-{Guid.NewGuid():N}");
        var inactiveRowVersion = (await GetVideoAsync(accessToken, inactive.Id)).RowVersion;
        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/videos/{inactive.Id}/deactivate", accessToken,
            new DeactivateVideoRequest(inactiveRowVersion)));

        var publicResponse = await _client.GetAsync("/api/v1/public/videos?lang=tr&pageSize=100");
        Assert.Equal(HttpStatusCode.OK, publicResponse.StatusCode);
        var publicResult = await publicResponse.Content.ReadFromJsonAsync<PagedResult<PublicVideoResponse>>();

        Assert.Contains(publicResult!.Items, i => i.Id == active.Id);
        Assert.DoesNotContain(publicResult.Items, i => i.Id == inactive.Id);
    }

    [Fact]
    public async Task PublicVideos_ExcludesVideosWithoutATranslationInTheRequestedLanguage()
    {
        var accessToken = await LoginAsAdminAsync();

        // "en" is seeded inactive in this fixture (reused as the shared "inactive language" fixture
        // across every Website flow test class), so requesting it would silently fall back to the
        // default language ("tr") instead of actually testing an active, non-default language - a
        // fresh one is created here instead, following the established pattern (e.g.
        // ResolveRouteFlowTests) of never activating the shared "en" to avoid cross-test interference.
        // LanguageCode requires a letters-only BCP-47-style code, so a GUID-derived code (which can
        // contain digits) is not usable here - "xyz" is unique within this test class (only this test
        // creates a language) and does not collide with any seeded code ("tr"/"en").
        const string languageCode = "xyz";
        var createLanguageResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/languages", accessToken,
            new CreateSiteLanguageRequest(languageCode, "Test Dili", 50)));
        Assert.Equal(HttpStatusCode.Created, createLanguageResponse.StatusCode);

        var created = await CreateVideoAsync(accessToken, title: $"SadeceTurkce-{Guid.NewGuid():N}");

        var publicResponse = await _client.GetAsync($"/api/v1/public/videos?lang={languageCode}&pageSize=100");
        var publicResult = await publicResponse.Content.ReadFromJsonAsync<PagedResult<PublicVideoResponse>>();

        Assert.DoesNotContain(publicResult!.Items, i => i.Id == created.Id);
    }

    [Fact]
    public async Task Activate_ReactivatesADeactivatedVideo()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateVideoAsync(accessToken);
        var rowVersion = (await GetVideoAsync(accessToken, created.Id)).RowVersion;

        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/videos/{created.Id}/deactivate", accessToken,
            new DeactivateVideoRequest(rowVersion)));
        var afterDeactivate = await GetVideoAsync(accessToken, created.Id);
        Assert.False(afterDeactivate.IsActive);

        var activateResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/videos/{created.Id}/activate", accessToken,
            new ActivateVideoRequest(afterDeactivate.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, activateResponse.StatusCode);

        var afterActivate = await GetVideoAsync(accessToken, created.Id);
        Assert.True(afterActivate.IsActive);
    }

    [Fact]
    public async Task DeleteVideo_WhenNotInUse_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateVideoAsync(accessToken);

        var deleteResponse = await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/videos/{created.Id}", accessToken));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDeleteResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/videos/{created.Id}", accessToken));
        Assert.Equal(HttpStatusCode.NotFound, afterDeleteResponse.StatusCode);
    }
}
