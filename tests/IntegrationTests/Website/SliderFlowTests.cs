using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.CreateSlider;
using GenclikMerkezi.Modules.Website.Features.DeleteSlider;
using GenclikMerkezi.Modules.Website.Features.GetSliderById;
using GenclikMerkezi.Modules.Website.Features.GetSliders;
using GenclikMerkezi.Modules.Website.Features.ReplaceSlides;
using GenclikMerkezi.Modules.Website.Features.UpdateSlider;
using GenclikMerkezi.Modules.Website.Features.UploadMediaAsset;
using SkiaSharp;

namespace GenclikMerkezi.IntegrationTests.Website;

// Faz 2 Görev 2. Shares the "one CustomWebApplicationFactory/one Sqlite database per class" caveat
// every other Website flow test class documents.
public class SliderFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SliderFlowTests(CustomWebApplicationFactory factory)
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
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(CreateJpeg(2000, 1000));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "slide.jpg");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/website/media") { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var uploaded = await response.Content.ReadFromJsonAsync<UploadMediaAssetResponse>();
        return uploaded!.Id;
    }

    private async Task<CreateSliderResponse> CreateSliderAsync(string accessToken, string? key = null, string name = "Ana Sayfa Hero")
    {
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/sliders", accessToken,
            new CreateSliderRequest(key ?? $"slider-{Guid.NewGuid():N}"[..20], name)));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CreateSliderResponse>())!;
    }

    private async Task<SliderDetailResponse> GetSliderAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/sliders/{id}", accessToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<SliderDetailResponse>())!;
    }

    private async Task<HttpResponseMessage> ReplaceSlidesAsync(
        string accessToken, Guid sliderId, byte[] rowVersion, IReadOnlyList<SlideInput> slides) =>
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/sliders/{sliderId}/slides", accessToken,
            new ReplaceSlidesRequest(rowVersion, slides)));

    private static SlideInput Slide(Guid desktopImageId, SlideLinkInput? link = null, string? buttonLabel = null, int sortOrder = 1) => new(
        desktopImageId, null, link, sortOrder, true, null, null,
        [new SlideTranslationInput("tr", null, "Başlık", null, buttonLabel, null)]);

    [Fact]
    public async Task CreateSlider_WithValidKeyAndName_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();

        var created = await CreateSliderAsync(accessToken, key: "home-hero-" + Guid.NewGuid().ToString("N")[..8]);
        var detail = await GetSliderAsync(accessToken, created.Id);

        Assert.Equal(created.Key, detail.Key);
        Assert.Single(detail.Translations, t => t.LanguageCode == "tr" && t.Name == "Ana Sayfa Hero");
        Assert.Empty(detail.Slides);
    }

    [Fact]
    public async Task CreateSlider_WithDuplicateKey_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = "duplicate-" + Guid.NewGuid().ToString("N")[..8];
        await CreateSliderAsync(accessToken, key: key);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/sliders", accessToken, new CreateSliderRequest(key, "Başka Ad")));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateSlider_WithInvalidKey_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/sliders", accessToken, new CreateSliderRequest("Invalid Key!", "Ad")));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetSliders_IncludesCreatedSlider()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateSliderAsync(accessToken);

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/sliders", accessToken));
        var sliders = await response.Content.ReadFromJsonAsync<List<SliderSummaryResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(sliders!, s => s.Id == created.Id);
    }

    [Fact]
    public async Task UpdateSlider_ReplacesNameTranslations()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateSliderAsync(accessToken);
        var detail = await GetSliderAsync(accessToken, created.Id);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/sliders/{created.Id}", accessToken,
            new UpdateSliderRequest(detail.RowVersion, [new SliderTranslationInput("tr", "Güncel Ad")])));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var updated = await GetSliderAsync(accessToken, created.Id);
        Assert.Single(updated.Translations, t => t.LanguageCode == "tr" && t.Name == "Güncel Ad");
    }

    [Fact]
    public async Task UpdateSlider_WithStaleRowVersion_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateSliderAsync(accessToken);
        var staleRowVersion = (await GetSliderAsync(accessToken, created.Id)).RowVersion;

        var firstUpdate = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/sliders/{created.Id}", accessToken,
            new UpdateSliderRequest(staleRowVersion, [new SliderTranslationInput("tr", "Birinci Güncelleme")])));
        Assert.Equal(HttpStatusCode.NoContent, firstUpdate.StatusCode);

        var secondUpdate = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/sliders/{created.Id}", accessToken,
            new UpdateSliderRequest(staleRowVersion, [new SliderTranslationInput("tr", "İkinci Güncelleme")])));
        Assert.Equal(HttpStatusCode.Conflict, secondUpdate.StatusCode);
    }

    [Fact]
    public async Task ReplaceSlides_WithValidSlides_FullLifecycle()
    {
        var accessToken = await LoginAsAdminAsync();
        var imageId = await UploadImageAsync(accessToken);
        var created = await CreateSliderAsync(accessToken);
        var detail = await GetSliderAsync(accessToken, created.Id);

        var firstReplace = await ReplaceSlidesAsync(accessToken, created.Id, detail.RowVersion, [Slide(imageId, sortOrder: 1)]);
        Assert.Equal(HttpStatusCode.NoContent, firstReplace.StatusCode);

        var afterFirst = await GetSliderAsync(accessToken, created.Id);
        Assert.Single(afterFirst.Slides);

        var secondReplace = await ReplaceSlidesAsync(
            accessToken, created.Id, afterFirst.RowVersion,
            [Slide(imageId, sortOrder: 1), Slide(imageId, sortOrder: 2)]);
        Assert.Equal(HttpStatusCode.NoContent, secondReplace.StatusCode);

        var afterSecond = await GetSliderAsync(accessToken, created.Id);
        Assert.Equal(2, afterSecond.Slides.Count);
    }

    [Fact]
    public async Task ReplaceSlides_WithStaleRowVersion_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var imageId = await UploadImageAsync(accessToken);
        var created = await CreateSliderAsync(accessToken);
        var staleRowVersion = (await GetSliderAsync(accessToken, created.Id)).RowVersion;

        var firstReplace = await ReplaceSlidesAsync(accessToken, created.Id, staleRowVersion, [Slide(imageId)]);
        Assert.Equal(HttpStatusCode.NoContent, firstReplace.StatusCode);

        var secondReplace = await ReplaceSlidesAsync(accessToken, created.Id, staleRowVersion, [Slide(imageId), Slide(imageId, sortOrder: 2)]);
        Assert.Equal(HttpStatusCode.Conflict, secondReplace.StatusCode);
    }

    [Fact]
    public async Task ReplaceSlides_WithButtonLabelButNoLink_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var imageId = await UploadImageAsync(accessToken);
        var created = await CreateSliderAsync(accessToken);
        var detail = await GetSliderAsync(accessToken, created.Id);

        var response = await ReplaceSlidesAsync(
            accessToken, created.Id, detail.RowVersion, [Slide(imageId, buttonLabel: "Devamını Gör")]);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ReplaceSlides_WithMoreThanMaxSlides_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var imageId = await UploadImageAsync(accessToken);
        var created = await CreateSliderAsync(accessToken);
        var detail = await GetSliderAsync(accessToken, created.Id);

        var slides = Enumerable.Range(1, 21).Select(i => Slide(imageId, sortOrder: i)).ToList();
        var response = await ReplaceSlidesAsync(accessToken, created.Id, detail.RowVersion, slides);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMediaAsset_WhenUsedAsSlideDesktopImage_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var imageId = await UploadImageAsync(accessToken);
        var created = await CreateSliderAsync(accessToken);
        var detail = await GetSliderAsync(accessToken, created.Id);
        await ReplaceSlidesAsync(accessToken, created.Id, detail.RowVersion, [Slide(imageId)]);

        var response = await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/media/{imageId}", accessToken));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteSlider_WhenNotInUse_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateSliderAsync(accessToken);

        var deleteResponse = await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/sliders/{created.Id}", accessToken));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDeleteResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/sliders/{created.Id}", accessToken));
        Assert.Equal(HttpStatusCode.NotFound, afterDeleteResponse.StatusCode);
    }
}
