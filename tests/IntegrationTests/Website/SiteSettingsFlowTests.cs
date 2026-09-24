using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.CreateSiteLanguage;
using GenclikMerkezi.Modules.Website.Features.GetPublicSite;
using GenclikMerkezi.Modules.Website.Features.GetSiteLanguages;
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
        Assert.Equal(string.Empty, body.TurnstileSiteKey);
    }

    [Fact]
    public async Task GetPublicSite_BeforeAnyUpdate_ResolvesToTheDefaultLanguage()
    {
        var response = await _client.GetAsync("/api/v1/public/site");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PublicSiteResponse>();
        Assert.NotNull(body);
        Assert.Equal("tr", body!.ResolvedLanguageCode);
        Assert.Contains(body.Languages, l => l.Code == "tr" && l.IsDefault);
        Assert.Empty(body.BankAccounts);
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

    private async Task ActivateEnglishAsync(string accessToken)
    {
        var listRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/admin/website/languages");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var listResponse = await _client.SendAsync(listRequest);
        var languages = await listResponse.Content.ReadFromJsonAsync<GetSiteLanguagesResponse>();
        var english = languages!.Items.Single(l => l.Code == "en");

        var activateRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/admin/website/languages/{english.Id}/activate");
        activateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        await _client.SendAsync(activateRequest);
    }

    private async Task<HttpResponseMessage> UpdateSettingsAsync(string accessToken, UpdateSiteSettingsRequest request)
    {
        var updateRequest = new HttpRequestMessage(HttpMethod.Put, "/api/v1/admin/website/settings") { Content = JsonContent.Create(request) };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return await _client.SendAsync(updateRequest);
    }

    private static UpdateSiteSettingsRequest BuildMinimalRequest(
        UpdateSiteSettingsThemeInput? theme = null,
        IReadOnlyList<UpdateSiteSettingsTranslationInput>? translations = null,
        IReadOnlyList<UpdateSiteSettingsBankAccountInput>? bankAccounts = null,
        bool donationPageEnabled = false,
        bool maintenanceModeEnabled = false,
        string? turnstileSiteKey = null) =>
        new(
            theme ?? new UpdateSiteSettingsThemeInput(null, null, null, null, null, null),
            new UpdateSiteSettingsContactInput(null, null, null, null, null),
            [],
            bankAccounts ?? [],
            translations ?? [],
            new UpdateSiteSettingsFeatureFlagsInput(false, false, false, donationPageEnabled, true),
            maintenanceModeEnabled,
            turnstileSiteKey);

    [Fact]
    public async Task UpdateSiteSettings_ThenReadBackFromAdminAndPublicEndpoints_ReflectsEveryChangedSection()
    {
        var accessToken = await LoginAsAdminAsync();
        var logoId = await UploadLogoAsync(accessToken);

        var updateResponse = await UpdateSettingsAsync(accessToken, new UpdateSiteSettingsRequest(
            new UpdateSiteSettingsThemeInput(logoId, null, null, "#123456", "#abcdef", "Inter"),
            new UpdateSiteSettingsContactInput("Ankara", "+90 555 000 00 00", "info@example.org", null, null),
            [new UpdateSiteSettingsSocialLinkInput("Instagram", "https://instagram.com/x", 1)],
            [new UpdateSiteSettingsBankAccountInput("TR330006100519786457841326", "Ziraat", "Dernek", "Genel bağış", 1, true)],
            [new UpdateSiteSettingsTranslationInput("tr", "Gençlik Merkezi", "Ana Sayfa", "Açıklama", "Footer metni", "Bakımdayız")],
            new UpdateSiteSettingsFeatureFlagsInput(true, false, false, true, true),
            true,
            "0x4AAA-site-key"));
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
        Assert.Equal("0x4AAA-site-key", adminBody.TurnstileSiteKey);
        var trTranslation = Assert.Single(adminBody.Translations, t => t.LanguageCode == "tr");
        Assert.Equal("Gençlik Merkezi", trTranslation.SiteName);
        Assert.Equal("Bakımdayız", trTranslation.MaintenanceMessage);

        var publicResponse = await _client.GetAsync("/api/v1/public/site");
        var publicBody = await publicResponse.Content.ReadFromJsonAsync<PublicSiteResponse>();
        Assert.Equal("#123456", publicBody!.Theme.PrimaryColorHex);
        Assert.NotNull(publicBody.Theme.LogoLightUrl);
        Assert.Single(publicBody.BankAccounts, a => a.Iban == "TR330006100519786457841326");
        Assert.True(publicBody.MaintenanceModeEnabled);
        Assert.Equal("Bakımdayız", publicBody.MaintenanceMessage);
        Assert.Equal("0x4AAA-site-key", publicBody.TurnstileSiteKey);
    }

    [Fact]
    public async Task UpdateSiteSettings_InvalidatesThePreviouslyCachedPublicSiteResponse()
    {
        var accessToken = await LoginAsAdminAsync();

        var beforeResponse = await _client.GetAsync("/api/v1/public/site");
        var before = await beforeResponse.Content.ReadFromJsonAsync<PublicSiteResponse>();
        Assert.NotEqual("#00ff00", before!.Theme.PrimaryColorHex);

        var updateResponse = await UpdateSettingsAsync(
            accessToken, BuildMinimalRequest(theme: new UpdateSiteSettingsThemeInput(null, null, null, "#00ff00", null, null)));
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var afterResponse = await _client.GetAsync("/api/v1/public/site");
        var after = await afterResponse.Content.ReadFromJsonAsync<PublicSiteResponse>();
        Assert.Equal("#00ff00", after!.Theme.PrimaryColorHex);
    }

    [Fact]
    public async Task GetPublicSite_WithUnknownLang_FallsBackToTheDefaultLanguage()
    {
        var response = await _client.GetAsync("/api/v1/public/site?lang=xx");
        var body = await response.Content.ReadFromJsonAsync<PublicSiteResponse>();

        Assert.Equal("tr", body!.ResolvedLanguageCode);
    }

    [Fact]
    public async Task GetPublicSite_WithInactiveLang_FallsBackToTheDefaultLanguage()
    {
        var accessToken = await LoginAsAdminAsync();

        // A freshly created language starts active (Görev 2) - deactivate it so this test owns its
        // own inactive language instead of depending on "en" staying untouched by other tests that
        // share this same singleton-backed factory (SiteLanguageDefaultsTests' own class avoids this
        // exact problem for SiteSettings; here the SiteLanguage table is the shared mutable state).
        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/website/languages")
        {
            Content = JsonContent.Create(new CreateSiteLanguageRequest("zz", "Test Dili", 99)),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var createResponse = await _client.SendAsync(createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateSiteLanguageResponse>();

        var deactivateRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/admin/website/languages/{created!.Id}/deactivate");
        deactivateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        await _client.SendAsync(deactivateRequest);

        var response = await _client.GetAsync("/api/v1/public/site?lang=zz");
        var body = await response.Content.ReadFromJsonAsync<PublicSiteResponse>();

        Assert.Equal("tr", body!.ResolvedLanguageCode);
    }

    [Fact]
    public async Task GetPublicSite_WithActiveLang_ResolvesToThatLanguagesOwnTranslation()
    {
        var accessToken = await LoginAsAdminAsync();
        await ActivateEnglishAsync(accessToken);

        var updateResponse = await UpdateSettingsAsync(accessToken, BuildMinimalRequest(translations:
        [
            new UpdateSiteSettingsTranslationInput("tr", "Gençlik Merkezi", null, null, null, null),
            new UpdateSiteSettingsTranslationInput("en", "Youth Center", null, null, null, null),
        ]));
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var response = await _client.GetAsync("/api/v1/public/site?lang=en");
        var body = await response.Content.ReadFromJsonAsync<PublicSiteResponse>();

        Assert.Equal("en", body!.ResolvedLanguageCode);
        Assert.Equal("Youth Center", body.SiteName);
        Assert.Contains(body.Languages, l => l.Code == "en");
    }

    [Fact]
    public async Task GetPublicSite_WithDonationPageDisabled_NeverReturnsBankAccounts()
    {
        var accessToken = await LoginAsAdminAsync();

        var updateResponse = await UpdateSettingsAsync(accessToken, BuildMinimalRequest(
            bankAccounts: [new UpdateSiteSettingsBankAccountInput("TR330006100519786457841326", "Ziraat", "Dernek", null, 1, true)],
            donationPageEnabled: false));
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var response = await _client.GetAsync("/api/v1/public/site");
        var body = await response.Content.ReadFromJsonAsync<PublicSiteResponse>();

        Assert.False(body!.DonationPageEnabled);
        Assert.Empty(body.BankAccounts);
    }

    [Fact]
    public async Task GetPublicSite_WithDonationPageEnabled_ReturnsOnlyActiveBankAccounts()
    {
        var accessToken = await LoginAsAdminAsync();

        var updateResponse = await UpdateSettingsAsync(accessToken, BuildMinimalRequest(
            bankAccounts:
            [
                new UpdateSiteSettingsBankAccountInput("TR330006100519786457841326", "Ziraat", "Dernek", null, 1, true),
                new UpdateSiteSettingsBankAccountInput("TR320010009999901234567890", "Halkbank", "Dernek", null, 2, false),
            ],
            donationPageEnabled: true));
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var response = await _client.GetAsync("/api/v1/public/site");
        var body = await response.Content.ReadFromJsonAsync<PublicSiteResponse>();

        Assert.True(body!.DonationPageEnabled);
        var account = Assert.Single(body.BankAccounts);
        Assert.Equal("TR330006100519786457841326", account.Iban);
    }

    [Fact]
    public async Task UpdateSiteSettings_WithLogoReferencingUnknownMediaAsset_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();

        var response = await UpdateSettingsAsync(
            accessToken, BuildMinimalRequest(theme: new UpdateSiteSettingsThemeInput(Guid.NewGuid(), null, null, null, null, null)));

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
            Content = JsonContent.Create(BuildMinimalRequest()),
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

        await UpdateSettingsAsync(accessToken, BuildMinimalRequest(theme: new UpdateSiteSettingsThemeInput(logoId, null, null, null, null, null)));

        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/admin/website/media/{logoId}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);

        Assert.Equal(HttpStatusCode.Conflict, deleteResponse.StatusCode);
    }
}
