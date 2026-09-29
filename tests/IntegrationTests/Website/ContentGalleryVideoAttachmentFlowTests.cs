using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.CreateVideo;
using GenclikMerkezi.Modules.Website.Features.GetContentItemById;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.SetContentItemAttachments;
using GenclikMerkezi.Modules.Website.Features.SetContentItemGallery;
using GenclikMerkezi.Modules.Website.Features.SetContentItemVideos;
using GenclikMerkezi.Modules.Website.Features.UploadMediaAsset;
using SkiaSharp;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §4.1 (Faz 1b Görev 4). Shares the "one CustomWebApplicationFactory/one Sqlite database per
// class" caveat every other Website flow test class documents - "news" (Gallery + Videos + Attachments
// all enabled) and "team" (none of the three) are the seeded content types this class relies on.
public class ContentGalleryVideoAttachmentFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private const string ValidVideoUrl = "https://youtu.be/dQw4w9WgXcQ";

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ContentGalleryVideoAttachmentFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<Guid> CreateContentItemAsync(string accessToken, Guid contentTypeId, string title = "Başlık")
    {
        var emptySeo = new CreateContentItemSeoInput(null, null, null, null, null, null, null, false);
        var slug = $"slug-{Guid.NewGuid():N}";
        var request = new CreateContentItemRequest(contentTypeId, null, 1, false, null, null, title, slug, null, null, emptySeo);
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, request));
        var created = await response.Content.ReadFromJsonAsync<CreateContentItemResponse>();
        return created!.Id;
    }

    private async Task<ContentItemDetailResponse> GetContentItemAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{id}", accessToken));
        return (await response.Content.ReadFromJsonAsync<ContentItemDetailResponse>())!;
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
        var fileContent = new ByteArrayContent(CreateJpeg(800, 600));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "gallery.jpg");

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

    private async Task<Guid> CreateVideoAsync(string accessToken, string title = "Video")
    {
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/videos", accessToken,
            new CreateVideoRequest(ValidVideoUrl, null, 1, title, null)));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreateVideoResponse>();
        return created!.Id;
    }

    // --- Gallery ---

    [Fact]
    public async Task SetGallery_ForTypeWithoutSupportsGallery_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var teamTypeId = await GetContentTypeIdByKeyAsync(accessToken, "team");
        var itemId = await CreateContentItemAsync(accessToken, teamTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        var mediaId = await UploadImageAsync(accessToken);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/gallery", accessToken,
            new SetContentItemGalleryRequest(item.RowVersion, [new GalleryItemInput(mediaId, 0, [])])));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetGallery_WithDocumentMediaAsset_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        var documentId = await UploadDocumentAsync(accessToken);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/gallery", accessToken,
            new SetContentItemGalleryRequest(item.RowVersion, [new GalleryItemInput(documentId, 0, [])])));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetGallery_WithDuplicateMediaAsset_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        var mediaId = await UploadImageAsync(accessToken);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/gallery", accessToken,
            new SetContentItemGalleryRequest(
                item.RowVersion, [new GalleryItemInput(mediaId, 0, []), new GalleryItemInput(mediaId, 1, [])])));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetGallery_WithValidItems_PersistsOrderAndOverrides()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        var firstMediaId = await UploadImageAsync(accessToken);
        var secondMediaId = await UploadImageAsync(accessToken);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/gallery", accessToken,
            new SetContentItemGalleryRequest(
                item.RowVersion,
                [
                    new GalleryItemInput(secondMediaId, 1, [new GalleryItemTranslationInput("tr", "Alt metin", "Açıklama")]),
                    new GalleryItemInput(firstMediaId, 0, []),
                ])));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var updated = await GetContentItemAsync(accessToken, itemId);
        Assert.Equal(2, updated.GalleryItems.Count);
        Assert.Equal(firstMediaId, updated.GalleryItems[0].MediaAssetId);
        Assert.Equal(secondMediaId, updated.GalleryItems[1].MediaAssetId);
        var secondTranslation = Assert.Single(updated.GalleryItems[1].Translations);
        Assert.Equal("Alt metin", secondTranslation.AltTextOverride);
        Assert.Equal("Açıklama", secondTranslation.CaptionOverride);
    }

    [Fact]
    public async Task SetGallery_WithStaleRowVersion_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        var mediaId = await UploadImageAsync(accessToken);
        var staleRowVersion = item.RowVersion;

        var firstResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/gallery", accessToken,
            new SetContentItemGalleryRequest(staleRowVersion, [new GalleryItemInput(mediaId, 0, [])])));
        Assert.Equal(HttpStatusCode.NoContent, firstResponse.StatusCode);

        var secondResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/gallery", accessToken,
            new SetContentItemGalleryRequest(staleRowVersion, [new GalleryItemInput(mediaId, 0, [])])));

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteMediaAsset_WhenUsedInGallery_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        var mediaId = await UploadImageAsync(accessToken);
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/gallery", accessToken,
            new SetContentItemGalleryRequest(item.RowVersion, [new GalleryItemInput(mediaId, 0, [])])));

        var deleteResponse = await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/media/{mediaId}", accessToken));

        Assert.Equal(HttpStatusCode.Conflict, deleteResponse.StatusCode);
    }

    // --- Videos ---

    [Fact]
    public async Task SetVideos_ForTypeWithoutSupportsVideos_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var teamTypeId = await GetContentTypeIdByKeyAsync(accessToken, "team");
        var itemId = await CreateContentItemAsync(accessToken, teamTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        var videoId = await CreateVideoAsync(accessToken);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/videos", accessToken,
            new SetContentItemVideosRequest(item.RowVersion, [videoId])));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetVideos_WithNonExistentVideoId_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/videos", accessToken,
            new SetContentItemVideosRequest(item.RowVersion, [Guid.NewGuid()])));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SetVideos_WithDuplicateVideoId_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        var videoId = await CreateVideoAsync(accessToken);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/videos", accessToken,
            new SetContentItemVideosRequest(item.RowVersion, [videoId, videoId])));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetVideos_WithValidIds_PersistsOrder()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        var firstVideoId = await CreateVideoAsync(accessToken, "Birinci");
        var secondVideoId = await CreateVideoAsync(accessToken, "İkinci");

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/videos", accessToken,
            new SetContentItemVideosRequest(item.RowVersion, [secondVideoId, firstVideoId])));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var updated = await GetContentItemAsync(accessToken, itemId);
        Assert.Equal([secondVideoId, firstVideoId], updated.VideoIds);
    }

    [Fact]
    public async Task DeleteVideo_WhenUsedByContentItem_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        var videoId = await CreateVideoAsync(accessToken);
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/videos", accessToken,
            new SetContentItemVideosRequest(item.RowVersion, [videoId])));

        var deleteResponse = await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/videos/{videoId}", accessToken));

        Assert.Equal(HttpStatusCode.Conflict, deleteResponse.StatusCode);
    }

    // --- Attachments ---

    [Fact]
    public async Task SetAttachments_ForTypeWithoutSupportsAttachments_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var teamTypeId = await GetContentTypeIdByKeyAsync(accessToken, "team");
        var itemId = await CreateContentItemAsync(accessToken, teamTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        var documentId = await UploadDocumentAsync(accessToken);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/attachments", accessToken,
            new SetContentItemAttachmentsRequest(item.RowVersion, [new AttachmentInput(documentId, 0, [])])));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetAttachments_WithImageMediaAsset_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        var imageId = await UploadImageAsync(accessToken);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/attachments", accessToken,
            new SetContentItemAttachmentsRequest(item.RowVersion, [new AttachmentInput(imageId, 0, [])])));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetAttachments_WithDuplicateMediaAsset_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        var documentId = await UploadDocumentAsync(accessToken);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/attachments", accessToken,
            new SetContentItemAttachmentsRequest(
                item.RowVersion, [new AttachmentInput(documentId, 0, []), new AttachmentInput(documentId, 1, [])])));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetAttachments_WithValidItems_PersistsOrderAndOverrides()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        var firstDocumentId = await UploadDocumentAsync(accessToken);
        var secondDocumentId = await UploadDocumentAsync(accessToken);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/attachments", accessToken,
            new SetContentItemAttachmentsRequest(
                item.RowVersion,
                [
                    new AttachmentInput(secondDocumentId, 1, [new AttachmentTranslationInput("tr", "Özel ad")]),
                    new AttachmentInput(firstDocumentId, 0, []),
                ])));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var updated = await GetContentItemAsync(accessToken, itemId);
        Assert.Equal(2, updated.Attachments.Count);
        Assert.Equal(firstDocumentId, updated.Attachments[0].MediaAssetId);
        Assert.Equal(secondDocumentId, updated.Attachments[1].MediaAssetId);
        var secondTranslation = Assert.Single(updated.Attachments[1].Translations);
        Assert.Equal("Özel ad", secondTranslation.DisplayNameOverride);
    }

    [Fact]
    public async Task DeleteMediaAsset_WhenUsedAsAttachment_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        var documentId = await UploadDocumentAsync(accessToken);
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/attachments", accessToken,
            new SetContentItemAttachmentsRequest(item.RowVersion, [new AttachmentInput(documentId, 0, [])])));

        var deleteResponse = await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/media/{documentId}", accessToken));

        Assert.Equal(HttpStatusCode.Conflict, deleteResponse.StatusCode);
    }
}
