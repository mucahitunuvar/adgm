using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.GetMediaAssetById;
using GenclikMerkezi.Modules.Website.Features.GetMediaAssets;
using GenclikMerkezi.Modules.Website.Features.UpdateMediaAsset;
using GenclikMerkezi.Modules.Website.Features.UploadMediaAsset;
using GenclikMerkezi.SharedKernel.Results;
using SkiaSharp;

namespace GenclikMerkezi.IntegrationTests.Website;

public class MediaAssetCrudFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public MediaAssetCrudFlowTests(CustomWebApplicationFactory factory)
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

    private static MultipartFormDataContent BuildUploadContent(byte[] fileBytes, string? folder, string? altText, string? caption)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "landscape.jpg");

        if (folder is not null)
        {
            content.Add(new StringContent(folder), "folder");
        }

        if (altText is not null)
        {
            content.Add(new StringContent(altText), "altText");
        }

        if (caption is not null)
        {
            content.Add(new StringContent(caption), "caption");
        }

        return content;
    }

    [Fact]
    public async Task UploadListDetailUpdateDelete_FullLifecycle()
    {
        var accessToken = await LoginAsAdminAsync();

        // Upload - large enough (2000x1000) to generate all three variants.
        var uploadRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/website/media")
        {
            Content = BuildUploadContent(CreateJpeg(2000, 1000), "logos", "Test alt metni", "Test açıklama"),
        };
        uploadRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var uploadResponse = await _client.SendAsync(uploadRequest);
        Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
        var uploaded = await uploadResponse.Content.ReadFromJsonAsync<UploadMediaAssetResponse>();
        Assert.NotNull(uploaded);
        Assert.Equal(2000, uploaded!.Width);
        Assert.Equal(1000, uploaded.Height);
        Assert.Equal(3, uploaded.Variants.Count);

        // The public media root actually serves the generated original.
        var originalPath = new Uri(uploaded.OriginalUrl).AbsolutePath;
        var originalFileResponse = await _client.GetAsync(originalPath);
        Assert.Equal(HttpStatusCode.OK, originalFileResponse.StatusCode);

        // List - filtered by folder, should include the uploaded asset with alt text present.
        var listRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/admin/website/media?folder=logos");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var listResponse = await _client.SendAsync(listRequest);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var listBody = await listResponse.Content.ReadFromJsonAsync<PagedResult<MediaAssetSummaryResponse>>();
        Assert.Contains(listBody!.Items, i => i.Id == uploaded.Id && i.HasAltText);

        // Detail - includes the translation set at upload time and (Faz 0) an empty usage list.
        var detailRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/admin/website/media/{uploaded.Id}");
        detailRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var detailResponse = await _client.SendAsync(detailRequest);
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        var detail = await detailResponse.Content.ReadFromJsonAsync<MediaAssetDetailResponse>();
        Assert.NotNull(detail);
        Assert.Empty(detail!.Usages);
        var trTranslation = Assert.Single(detail.Translations, t => t.LanguageCode == "tr");
        Assert.Equal("Test alt metni", trTranslation.AltText);

        // Update - change folder, add an English translation.
        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/admin/website/media/{uploaded.Id}")
        {
            Content = JsonContent.Create(new UpdateMediaAssetRequest(
                "partners", "Unsplash", null, false,
                [
                    new UpdateMediaAssetTranslationInput("tr", "Güncel alt metin", "Güncel açıklama"),
                    new UpdateMediaAssetTranslationInput("en", "Updated alt text", "Updated caption"),
                ])),
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var detailAfterUpdateRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/admin/website/media/{uploaded.Id}");
        detailAfterUpdateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var detailAfterUpdateResponse = await _client.SendAsync(detailAfterUpdateRequest);
        var detailAfterUpdate = await detailAfterUpdateResponse.Content.ReadFromJsonAsync<MediaAssetDetailResponse>();
        Assert.Equal("partners", detailAfterUpdate!.Folder);
        Assert.Equal(2, detailAfterUpdate.Translations.Count);
        Assert.Contains(detailAfterUpdate.Translations, t => t.LanguageCode == "en" && t.AltText == "Updated alt text");

        // Delete - no usages registered in Faz 0, so this must succeed.
        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/admin/website/media/{uploaded.Id}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDeleteRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/admin/website/media/{uploaded.Id}");
        afterDeleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var afterDeleteResponse = await _client.SendAsync(afterDeleteRequest);
        Assert.Equal(HttpStatusCode.NotFound, afterDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task UploadDocument_WithFakePdfSignature_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent("this is not a real pdf"u8.ToArray());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", "fake.pdf");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/website/media") { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadMedia_AsNonAdmin_ReturnsForbidden()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email, password = "Sifre123", firstName = "Test", lastName = "User", role = "Candidate" });
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Sifre123" });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/website/media")
        {
            Content = BuildUploadContent(CreateJpeg(100, 100), null, null, null),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
