using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.GetPublicSiteSettings;
using GenclikMerkezi.Modules.Website.Features.GetSiteSettings;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettings;
using GenclikMerkezi.Modules.Website.Features.UploadMediaAsset;
using SkiaSharp;

namespace GenclikMerkezi.IntegrationTests.Website;

// SiteSettings is a singleton row shared by every test that runs against the same
// CustomWebApplicationFactory instance (and hence the same SQLite database) - xUnit gives each test
// *class* its own factory instance, but methods within one class share it and can run in any order.
// The "before any update" assertions below therefore live in their own class so nothing in
// SiteSettingsFlowTests (which deliberately mutates the singleton) can race them.
public class SiteSettingsDefaultsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SiteSettingsDefaultsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSiteSettings_BeforeAnyUpdate_ReturnsAdrSpecifiedDefaults()
    {
        var email = $"admin-{Guid.NewGuid():N}@example.com";
        const string password = "AdminSifre123";
        await _factory.SeedAdminUserAsync(email, password);
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/admin/website/settings");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<SiteSettingsResponse>();
        Assert.True(body!.BotProtectionEnabled);
        Assert.False(body.DonationPageEnabled);
        Assert.Empty(body.BankAccounts);
    }

    [Fact]
    public async Task GetPublicSiteSettings_IsReachableWithoutAuthentication()
    {
        var response = await _client.GetAsync("/api/v1/public/website/settings");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PublicSiteSettingsResponse>();
        Assert.NotNull(body);
    }
}

public class SiteSettingsFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SiteSettingsFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<Guid> UploadLogoAsync(string accessToken)
    {
        using var bitmap = new SKBitmap(200, 200);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(data.ToArray());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "logo.png");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/website/media") { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _client.SendAsync(request);
        var uploaded = await response.Content.ReadFromJsonAsync<UploadMediaAssetResponse>();
        return uploaded!.Id;
    }

    [Fact]
    public async Task UpdateSiteSettings_ThenReadBackFromAdminAndPublicEndpoints_ReflectsEveryChangedSection()
    {
        var accessToken = await LoginAsAdminAsync();
        var logoId = await UploadLogoAsync(accessToken);

        var updateRequest = new HttpRequestMessage(HttpMethod.Put, "/api/v1/admin/website/settings")
        {
            Content = JsonContent.Create(new UpdateSiteSettingsRequest(
                new UpdateSiteSettingsThemeInput(logoId, null, null, "#123456", "#abcdef", "Inter"),
                new UpdateSiteSettingsContactInput("Ankara", "+90 555 000 00 00", "info@example.org", null, null),
                [new UpdateSiteSettingsSocialLinkInput("Instagram", "https://instagram.com/x", 1)],
                [new UpdateSiteSettingsBankAccountInput("TR330006100519786457841326", "Ziraat", "Dernek", "Genel bağış", 1, true)],
                [new UpdateSiteSettingsTranslationInput("tr", "Gençlik Merkezi", "Ana Sayfa", "Açıklama", "Footer metni")],
                new UpdateSiteSettingsFeatureFlagsInput(true, false, false, false, true),
                true,
                "Bakımdayız")),
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var adminGetRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/admin/website/settings");
        adminGetRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var adminGetResponse = await _client.SendAsync(adminGetRequest);
        var adminBody = await adminGetResponse.Content.ReadFromJsonAsync<SiteSettingsResponse>();
        Assert.Equal("#123456", adminBody!.Theme.PrimaryColorHex);
        Assert.Equal(logoId, adminBody.Theme.LogoLightMediaAssetId);
        Assert.NotNull(adminBody.Theme.LogoLightUrl);
        Assert.Equal("info@example.org", adminBody.Contact.Email);
        Assert.True(adminBody.GlobalSearchEnabled);
        Assert.True(adminBody.MaintenanceModeEnabled);
        Assert.Equal("Bakımdayız", adminBody.MaintenanceMessage);
        var trTranslation = Assert.Single(adminBody.Translations, t => t.LanguageCode == "tr");
        Assert.Equal("Gençlik Merkezi", trTranslation.SiteName);

        var publicResponse = await _client.GetAsync("/api/v1/public/website/settings");
        var publicBody = await publicResponse.Content.ReadFromJsonAsync<PublicSiteSettingsResponse>();
        Assert.Equal("#123456", publicBody!.Theme.PrimaryColorHex);
        Assert.NotNull(publicBody.Theme.LogoLightUrl);
        Assert.Single(publicBody.BankAccounts, a => a.Iban == "TR330006100519786457841326");
        Assert.True(publicBody.MaintenanceModeEnabled);
    }

    [Fact]
    public async Task UpdateSiteSettings_WithLogoReferencingUnknownMediaAsset_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();

        var updateRequest = new HttpRequestMessage(HttpMethod.Put, "/api/v1/admin/website/settings")
        {
            Content = JsonContent.Create(new UpdateSiteSettingsRequest(
                new UpdateSiteSettingsThemeInput(Guid.NewGuid(), null, null, null, null, null),
                new UpdateSiteSettingsContactInput(null, null, null, null, null),
                [],
                [],
                [],
                new UpdateSiteSettingsFeatureFlagsInput(false, false, false, false, true),
                false,
                null)),
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _client.SendAsync(updateRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSiteSettings_AsNonAdmin_ReturnsForbidden()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email, password = "Sifre123", firstName = "Test", lastName = "User", role = "Candidate" });
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Sifre123" });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var updateRequest = new HttpRequestMessage(HttpMethod.Put, "/api/v1/admin/website/settings")
        {
            Content = JsonContent.Create(new UpdateSiteSettingsRequest(
                new UpdateSiteSettingsThemeInput(null, null, null, null, null, null),
                new UpdateSiteSettingsContactInput(null, null, null, null, null),
                [],
                [],
                [],
                new UpdateSiteSettingsFeatureFlagsInput(false, false, false, false, true),
                false,
                null)),
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
        var response = await _client.SendAsync(updateRequest);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMediaAsset_WhenUsedAsSiteLogo_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var logoId = await UploadLogoAsync(accessToken);

        var updateRequest = new HttpRequestMessage(HttpMethod.Put, "/api/v1/admin/website/settings")
        {
            Content = JsonContent.Create(new UpdateSiteSettingsRequest(
                new UpdateSiteSettingsThemeInput(logoId, null, null, null, null, null),
                new UpdateSiteSettingsContactInput(null, null, null, null, null),
                [],
                [],
                [],
                new UpdateSiteSettingsFeatureFlagsInput(false, false, false, false, true),
                false,
                null)),
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        await _client.SendAsync(updateRequest);

        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/admin/website/media/{logoId}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);

        Assert.Equal(HttpStatusCode.Conflict, deleteResponse.StatusCode);
    }
}
