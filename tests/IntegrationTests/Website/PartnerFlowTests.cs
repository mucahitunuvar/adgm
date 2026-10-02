using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Application.Partners;
using GenclikMerkezi.Modules.Website.Features.ActivatePartner;
using GenclikMerkezi.Modules.Website.Features.CreatePartner;
using GenclikMerkezi.Modules.Website.Features.CreateSiteLanguage;
using GenclikMerkezi.Modules.Website.Features.DeactivatePartner;
using GenclikMerkezi.Modules.Website.Features.DeletePartnerTranslation;
using GenclikMerkezi.Modules.Website.Features.GetPartnerById;
using GenclikMerkezi.Modules.Website.Features.GetPartners;
using GenclikMerkezi.Modules.Website.Features.UpdatePartner;
using GenclikMerkezi.Modules.Website.Features.UpdatePartnerTranslation;
using GenclikMerkezi.Modules.Website.Features.UploadMediaAsset;
using GenclikMerkezi.SharedKernel.Results;
using SkiaSharp;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §8.2 (Faz 2 Görev 3). Shares the "one CustomWebApplicationFactory/one Sqlite database per
// class" caveat every other Website flow test class documents.
public class PartnerFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PartnerFlowTests(CustomWebApplicationFactory factory)
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
        content.Add(fileContent, "file", "logo.jpg");

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

    private async Task<CreatePartnerResponse> CreatePartnerAsync(
        string accessToken, Guid logoMediaId, string? websiteUrl = "https://example.com", string name = "Örnek Partner")
    {
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/partners", accessToken,
            new CreatePartnerRequest(logoMediaId, websiteUrl, 1, name, null)));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CreatePartnerResponse>())!;
    }

    private async Task<PartnerDetailResponse> GetPartnerAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/partners/{id}", accessToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<PartnerDetailResponse>())!;
    }

    [Fact]
    public async Task CreatePartner_WithValidHttpsUrl_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var logoId = await UploadImageAsync(accessToken);

        var created = await CreatePartnerAsync(accessToken, logoId);
        var detail = await GetPartnerAsync(accessToken, created.Id);

        Assert.Equal("https://example.com", detail.WebsiteUrl);
        Assert.True(detail.IsActive);
        Assert.NotNull(detail.Logo);
        Assert.Single(detail.Translations, t => t.LanguageCode == "tr" && t.Name == "Örnek Partner");
    }

    [Fact]
    public async Task CreatePartner_WithNonHttpsUrl_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var logoId = await UploadImageAsync(accessToken);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/partners", accessToken,
            new CreatePartnerRequest(logoId, "http://example.com", 1, "Örnek Partner", null)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePartner_WithDocumentAsLogo_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var documentId = await UploadDocumentAsync(accessToken);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/partners", accessToken,
            new CreatePartnerRequest(documentId, null, 1, "Örnek Partner", null)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMediaAsset_WhenUsedAsPartnerLogo_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var logoId = await UploadImageAsync(accessToken);
        await CreatePartnerAsync(accessToken, logoId);

        var response = await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/media/{logoId}", accessToken));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePartner_WithStaleRowVersion_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var logoId = await UploadImageAsync(accessToken);
        var created = await CreatePartnerAsync(accessToken, logoId);
        var staleRowVersion = (await GetPartnerAsync(accessToken, created.Id)).RowVersion;

        var firstUpdate = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/partners/{created.Id}", accessToken,
            new UpdatePartnerRequest(staleRowVersion, logoId, "https://example.com", 2)));
        Assert.Equal(HttpStatusCode.NoContent, firstUpdate.StatusCode);

        var secondUpdate = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/partners/{created.Id}", accessToken,
            new UpdatePartnerRequest(staleRowVersion, logoId, "https://example.com", 3)));

        Assert.Equal(HttpStatusCode.Conflict, secondUpdate.StatusCode);
    }

    [Fact]
    public async Task AddUpdateAndDeleteTranslation_FullLifecycle()
    {
        var accessToken = await LoginAsAdminAsync();
        var logoId = await UploadImageAsync(accessToken);
        var created = await CreatePartnerAsync(accessToken, logoId);
        var rowVersion = (await GetPartnerAsync(accessToken, created.Id)).RowVersion;

        var addResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/partners/{created.Id}/translations/en", accessToken,
            new UpdatePartnerTranslationRequest(rowVersion, "Example Partner", "Description")));
        Assert.Equal(HttpStatusCode.NoContent, addResponse.StatusCode);

        var afterAdd = await GetPartnerAsync(accessToken, created.Id);
        Assert.Equal(2, afterAdd.Translations.Count);
        Assert.Single(afterAdd.Translations, t => t.LanguageCode == "en" && t.Name == "Example Partner");

        var deleteResponse = await _client.SendAsync(Authorized(
            HttpMethod.Delete, $"/api/v1/admin/website/partners/{created.Id}/translations/en", accessToken,
            new DeletePartnerTranslationRequest(afterAdd.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDelete = await GetPartnerAsync(accessToken, created.Id);
        Assert.Single(afterDelete.Translations);
    }

    [Fact]
    public async Task DeleteTranslation_ForDefaultLanguage_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var logoId = await UploadImageAsync(accessToken);
        var created = await CreatePartnerAsync(accessToken, logoId);
        var rowVersion = (await GetPartnerAsync(accessToken, created.Id)).RowVersion;

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Delete, $"/api/v1/admin/website/partners/{created.Id}/translations/tr", accessToken,
            new DeletePartnerTranslationRequest(rowVersion)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetPartners_FiltersByIsActiveAndSearch()
    {
        var accessToken = await LoginAsAdminAsync();
        var logoId = await UploadImageAsync(accessToken);
        var uniqueName = $"Benzersiz-{Guid.NewGuid():N}";
        var created = await CreatePartnerAsync(accessToken, logoId, name: uniqueName);
        var rowVersion = (await GetPartnerAsync(accessToken, created.Id)).RowVersion;
        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/partners/{created.Id}/deactivate", accessToken,
            new DeactivatePartnerRequest(rowVersion)));

        var searchResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, $"/api/v1/admin/website/partners?search={uniqueName}", accessToken));
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<PagedResult<PartnerSummaryResponse>>();
        Assert.Single(searchResult!.Items, i => i.Id == created.Id);

        var activeOnlyResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, $"/api/v1/admin/website/partners?search={uniqueName}&isActive=true", accessToken));
        var activeOnlyResult = await activeOnlyResponse.Content.ReadFromJsonAsync<PagedResult<PartnerSummaryResponse>>();
        Assert.Empty(activeOnlyResult!.Items);
    }

    [Fact]
    public async Task PublicPartners_OnlyReturnsActivePartnersWithATranslationInTheRequestedLanguage()
    {
        var accessToken = await LoginAsAdminAsync();
        var logoId = await UploadImageAsync(accessToken);

        var active = await CreatePartnerAsync(accessToken, logoId, name: $"Aktif-{Guid.NewGuid():N}");

        var inactive = await CreatePartnerAsync(accessToken, logoId, name: $"Pasif-{Guid.NewGuid():N}");
        var inactiveRowVersion = (await GetPartnerAsync(accessToken, inactive.Id)).RowVersion;
        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/partners/{inactive.Id}/deactivate", accessToken,
            new DeactivatePartnerRequest(inactiveRowVersion)));

        var publicResponse = await _client.GetAsync("/api/v1/public/partners?lang=tr");
        Assert.Equal(HttpStatusCode.OK, publicResponse.StatusCode);
        var publicResult = await publicResponse.Content.ReadFromJsonAsync<IReadOnlyList<PublicPartnerResponse>>();

        Assert.Contains(publicResult!, i => i.Id == active.Id);
        Assert.DoesNotContain(publicResult!, i => i.Id == inactive.Id);
    }

    [Fact]
    public async Task PublicPartners_ExcludesPartnersWithoutATranslationInTheRequestedLanguage()
    {
        var accessToken = await LoginAsAdminAsync();

        // A fresh language avoids the shared "en" fixture (seeded inactive) and cross-test
        // interference - the same convention VideoFlowTests' own remarks describe.
        const string languageCode = "xyz";
        var createLanguageResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/languages", accessToken,
            new CreateSiteLanguageRequest(languageCode, "Test Dili", 50)));
        Assert.Equal(HttpStatusCode.Created, createLanguageResponse.StatusCode);

        var logoId = await UploadImageAsync(accessToken);
        var created = await CreatePartnerAsync(accessToken, logoId, name: $"SadeceTurkce-{Guid.NewGuid():N}");

        var publicResponse = await _client.GetAsync($"/api/v1/public/partners?lang={languageCode}");
        var publicResult = await publicResponse.Content.ReadFromJsonAsync<IReadOnlyList<PublicPartnerResponse>>();

        Assert.DoesNotContain(publicResult!, i => i.Id == created.Id);
    }

    [Fact]
    public async Task Activate_ReactivatesADeactivatedPartner()
    {
        var accessToken = await LoginAsAdminAsync();
        var logoId = await UploadImageAsync(accessToken);
        var created = await CreatePartnerAsync(accessToken, logoId);
        var rowVersion = (await GetPartnerAsync(accessToken, created.Id)).RowVersion;

        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/partners/{created.Id}/deactivate", accessToken,
            new DeactivatePartnerRequest(rowVersion)));
        var afterDeactivate = await GetPartnerAsync(accessToken, created.Id);
        Assert.False(afterDeactivate.IsActive);

        var activateResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/partners/{created.Id}/activate", accessToken,
            new ActivatePartnerRequest(afterDeactivate.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, activateResponse.StatusCode);

        var afterActivate = await GetPartnerAsync(accessToken, created.Id);
        Assert.True(afterActivate.IsActive);
    }

    [Fact]
    public async Task DeletePartner_WhenNotInUse_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var logoId = await UploadImageAsync(accessToken);
        var created = await CreatePartnerAsync(accessToken, logoId);

        var deleteResponse = await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/partners/{created.Id}", accessToken));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDeleteResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/partners/{created.Id}", accessToken));
        Assert.Equal(HttpStatusCode.NotFound, afterDeleteResponse.StatusCode);
    }
}
