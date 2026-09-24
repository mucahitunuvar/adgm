using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.CreateSiteLanguage;
using GenclikMerkezi.Modules.Website.Features.GetPublicSite;
using GenclikMerkezi.Modules.Website.Features.GetSiteLanguages;
using GenclikMerkezi.Modules.Website.Features.GetSiteSettings;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsBankAccounts;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsBotProtection;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsContact;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsFeatures;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsIdentity;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsMaintenance;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsTheme;
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
        Assert.NotEmpty(body.RowVersion);
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

    private async Task<byte[]> GetCurrentRowVersionAsync(string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/admin/website/settings");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<SiteSettingsResponse>();
        return body!.RowVersion;
    }

    private async Task<HttpResponseMessage> PutAsync<TRequest>(string accessToken, string path, TRequest request)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Put, path) { Content = JsonContent.Create(request) };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return await _client.SendAsync(httpRequest);
    }

    private Task<HttpResponseMessage> PutIdentityAsync(
        string accessToken, byte[] rowVersion, Guid? logoLightMediaAssetId = null,
        IReadOnlyList<UpdateSiteSettingsIdentityTranslationInput>? translations = null) =>
        PutAsync(accessToken, "/api/v1/admin/website/settings/identity", new UpdateSiteSettingsIdentityRequest(
            rowVersion, logoLightMediaAssetId, null, null, null, translations ?? []));

    private Task<HttpResponseMessage> PutThemeAsync(
        string accessToken, byte[] rowVersion, string? primaryColorHex = null, string? secondaryColorHex = null, string? fontFamily = null) =>
        PutAsync(accessToken, "/api/v1/admin/website/settings/theme",
            new UpdateSiteSettingsThemeRequest(rowVersion, primaryColorHex, secondaryColorHex, fontFamily));

    private Task<HttpResponseMessage> PutContactAsync(
        string accessToken, byte[] rowVersion, string? email = null,
        IReadOnlyList<GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsContact.UpdateSiteSettingsSocialLinkInput>? socialLinks = null) =>
        PutAsync(accessToken, "/api/v1/admin/website/settings/contact",
            new UpdateSiteSettingsContactRequest(rowVersion, null, null, email, null, null, socialLinks ?? []));

    private Task<HttpResponseMessage> PutBankAccountsAsync(
        string accessToken, byte[] rowVersion, IReadOnlyList<UpdateSiteSettingsBankAccountInput> bankAccounts) =>
        PutAsync(accessToken, "/api/v1/admin/website/settings/bank-accounts", new UpdateSiteSettingsBankAccountsRequest(rowVersion, bankAccounts));

    private Task<HttpResponseMessage> PutFeaturesAsync(
        string accessToken, byte[] rowVersion, bool donationPageEnabled = false, bool globalSearchEnabled = false) =>
        PutAsync(accessToken, "/api/v1/admin/website/settings/features",
            new UpdateSiteSettingsFeaturesRequest(rowVersion, globalSearchEnabled, false, false, donationPageEnabled));

    private Task<HttpResponseMessage> PutMaintenanceAsync(
        string accessToken, byte[] rowVersion, bool maintenanceModeEnabled,
        IReadOnlyList<UpdateSiteSettingsMaintenanceTranslationInput>? translations = null) =>
        PutAsync(accessToken, "/api/v1/admin/website/settings/maintenance",
            new UpdateSiteSettingsMaintenanceRequest(rowVersion, maintenanceModeEnabled, translations ?? []));

    private Task<HttpResponseMessage> PutBotProtectionAsync(string accessToken, byte[] rowVersion, bool enabled, string? siteKey) =>
        PutAsync(accessToken, "/api/v1/admin/website/settings/bot-protection", new UpdateSiteSettingsBotProtectionRequest(rowVersion, enabled, siteKey));

    [Fact]
    public async Task UpdateIdentity_ThenReadBack_ReflectsLogoAndTranslationsInBothAdminAndPublic()
    {
        var accessToken = await LoginAsAdminAsync();
        var logoId = await UploadLogoAsync(accessToken);
        var rowVersion = await GetCurrentRowVersionAsync(accessToken);

        var response = await PutIdentityAsync(accessToken, rowVersion, logoId,
            [new UpdateSiteSettingsIdentityTranslationInput("tr", "Gençlik Merkezi", "Birlikte güçlüyüz", "Ana Sayfa", "Açıklama", "Footer metni")]);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var adminGetRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/admin/website/settings");
        adminGetRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var adminBody = await (await _client.SendAsync(adminGetRequest)).Content.ReadFromJsonAsync<SiteSettingsResponse>();
        Assert.Equal(logoId, adminBody!.LogoLightMediaAssetId);
        Assert.NotNull(adminBody.LogoLightUrl);
        var trTranslation = Assert.Single(adminBody.Translations, t => t.LanguageCode == "tr");
        Assert.Equal("Gençlik Merkezi", trTranslation.SiteName);
        Assert.Equal("Birlikte güçlüyüz", trTranslation.Tagline);

        var publicBody = await (await _client.GetAsync("/api/v1/public/site")).Content.ReadFromJsonAsync<PublicSiteResponse>();
        Assert.NotNull(publicBody!.LogoLightUrl);
        Assert.Equal("Gençlik Merkezi", publicBody.SiteName);
        Assert.Equal("Birlikte güçlüyüz", publicBody.Tagline);
    }

    [Fact]
    public async Task UpdateTheme_ThenReadBack_ReflectsColorsInBothAdminAndPublic()
    {
        var accessToken = await LoginAsAdminAsync();
        var rowVersion = await GetCurrentRowVersionAsync(accessToken);

        var response = await PutThemeAsync(accessToken, rowVersion, "#123456", "#abcdef", "Inter");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var publicBody = await (await _client.GetAsync("/api/v1/public/site")).Content.ReadFromJsonAsync<PublicSiteResponse>();
        Assert.Equal("#123456", publicBody!.Theme.PrimaryColorHex);
        Assert.Equal("Inter", publicBody.Theme.FontFamily);
    }

    [Fact]
    public async Task UpdateContact_ThenReadBack_ReflectsContactAndSocialLinks()
    {
        var accessToken = await LoginAsAdminAsync();
        var rowVersion = await GetCurrentRowVersionAsync(accessToken);

        var response = await PutContactAsync(
            accessToken, rowVersion, "info@example.org",
            [new GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsContact.UpdateSiteSettingsSocialLinkInput("Instagram", "https://instagram.com/x", 1)]);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var publicBody = await (await _client.GetAsync("/api/v1/public/site")).Content.ReadFromJsonAsync<PublicSiteResponse>();
        Assert.Equal("info@example.org", publicBody!.Contact.Email);
        Assert.Single(publicBody.SocialLinks, l => l.Platform == "Instagram");
    }

    [Fact]
    public async Task UpdateBankAccounts_ThenReadBack_ReflectsAccountsIncludingCurrency()
    {
        var accessToken = await LoginAsAdminAsync();
        var rowVersion = await GetCurrentRowVersionAsync(accessToken);

        var response = await PutBankAccountsAsync(
            accessToken, rowVersion, [new UpdateSiteSettingsBankAccountInput("TR330006100519786457841326", "Ziraat", "Dernek", "TRY", "Genel bağış", 1, true)]);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var adminGetRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/admin/website/settings");
        adminGetRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var adminBody = await (await _client.SendAsync(adminGetRequest)).Content.ReadFromJsonAsync<SiteSettingsResponse>();
        var account = Assert.Single(adminBody!.BankAccounts);
        Assert.Equal("TRY", account.Currency);
    }

    [Fact]
    public async Task UpdateFeatures_ThenReadBack_ReflectsFlags()
    {
        var accessToken = await LoginAsAdminAsync();
        var rowVersion = await GetCurrentRowVersionAsync(accessToken);

        var response = await PutFeaturesAsync(accessToken, rowVersion, donationPageEnabled: true, globalSearchEnabled: true);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var publicBody = await (await _client.GetAsync("/api/v1/public/site")).Content.ReadFromJsonAsync<PublicSiteResponse>();
        Assert.True(publicBody!.DonationPageEnabled);
        Assert.True(publicBody.GlobalSearchEnabled);
    }

    [Fact]
    public async Task UpdateMaintenance_ThenReadBack_ReflectsGlobalSwitchAndPerLanguageMessage()
    {
        var accessToken = await LoginAsAdminAsync();
        var rowVersion = await GetCurrentRowVersionAsync(accessToken);

        var response = await PutMaintenanceAsync(
            accessToken, rowVersion, true, [new UpdateSiteSettingsMaintenanceTranslationInput("tr", "Bakımdayız")]);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var publicBody = await (await _client.GetAsync("/api/v1/public/site")).Content.ReadFromJsonAsync<PublicSiteResponse>();
        Assert.True(publicBody!.MaintenanceModeEnabled);
        Assert.Equal("Bakımdayız", publicBody.MaintenanceMessage);
    }

    [Fact]
    public async Task UpdateBotProtection_ThenReadBack_ReflectsFlagAndSiteKey()
    {
        var accessToken = await LoginAsAdminAsync();
        var rowVersion = await GetCurrentRowVersionAsync(accessToken);

        var response = await PutBotProtectionAsync(accessToken, rowVersion, false, "0x4AAA-site-key");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var adminGetRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/admin/website/settings");
        adminGetRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var adminBody = await (await _client.SendAsync(adminGetRequest)).Content.ReadFromJsonAsync<SiteSettingsResponse>();
        Assert.False(adminBody!.BotProtectionEnabled);
        Assert.Equal("0x4AAA-site-key", adminBody.TurnstileSiteKey);

        var publicBody = await (await _client.GetAsync("/api/v1/public/site")).Content.ReadFromJsonAsync<PublicSiteResponse>();
        Assert.Equal("0x4AAA-site-key", publicBody!.TurnstileSiteKey);
    }

    [Fact]
    public async Task UpdateTheme_WithStaleRowVersion_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var staleRowVersion = await GetCurrentRowVersionAsync(accessToken);

        // Someone else's edit lands first, advancing the real RowVersion.
        var firstResponse = await PutThemeAsync(accessToken, staleRowVersion, "#111111");
        Assert.Equal(HttpStatusCode.NoContent, firstResponse.StatusCode);

        // The caller still holds the RowVersion from before that edit.
        var conflictResponse = await PutThemeAsync(accessToken, staleRowVersion, "#222222");

        Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);

        // The first writer's value stuck - the stale write must not have applied.
        var publicBody = await (await _client.GetAsync("/api/v1/public/site")).Content.ReadFromJsonAsync<PublicSiteResponse>();
        Assert.Equal("#111111", publicBody!.Theme.PrimaryColorHex);
    }

    [Fact]
    public async Task UpdateSiteSettings_InvalidatesThePreviouslyCachedPublicSiteResponse()
    {
        var accessToken = await LoginAsAdminAsync();

        var before = await (await _client.GetAsync("/api/v1/public/site")).Content.ReadFromJsonAsync<PublicSiteResponse>();
        Assert.NotEqual("#00ff00", before!.Theme.PrimaryColorHex);

        var rowVersion = await GetCurrentRowVersionAsync(accessToken);
        var updateResponse = await PutThemeAsync(accessToken, rowVersion, "#00ff00");
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var after = await (await _client.GetAsync("/api/v1/public/site")).Content.ReadFromJsonAsync<PublicSiteResponse>();
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
        // share this same singleton-backed factory (SiteSettingsDefaultsTests' own class avoids this
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
        var rowVersion = await GetCurrentRowVersionAsync(accessToken);

        var updateResponse = await PutIdentityAsync(accessToken, rowVersion, translations:
        [
            new UpdateSiteSettingsIdentityTranslationInput("tr", "Gençlik Merkezi", null, null, null, null),
            new UpdateSiteSettingsIdentityTranslationInput("en", "Youth Center", null, null, null, null),
        ]);
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
        var rowVersion = await GetCurrentRowVersionAsync(accessToken);

        await PutBankAccountsAsync(accessToken, rowVersion, [new UpdateSiteSettingsBankAccountInput("TR330006100519786457841326", "Ziraat", "Dernek", "TRY", null, 1, true)]);
        rowVersion = await GetCurrentRowVersionAsync(accessToken);
        await PutFeaturesAsync(accessToken, rowVersion, donationPageEnabled: false);

        var body = await (await _client.GetAsync("/api/v1/public/site")).Content.ReadFromJsonAsync<PublicSiteResponse>();

        Assert.False(body!.DonationPageEnabled);
        Assert.Empty(body.BankAccounts);
    }

    [Fact]
    public async Task GetPublicSite_WithDonationPageEnabled_ReturnsOnlyActiveBankAccounts()
    {
        var accessToken = await LoginAsAdminAsync();
        var rowVersion = await GetCurrentRowVersionAsync(accessToken);

        await PutBankAccountsAsync(accessToken, rowVersion,
        [
            new UpdateSiteSettingsBankAccountInput("TR330006100519786457841326", "Ziraat", "Dernek", "TRY", null, 1, true),
            new UpdateSiteSettingsBankAccountInput("TR320010009999901234567890", "Halkbank", "Dernek", "TRY", null, 2, false),
        ]);
        rowVersion = await GetCurrentRowVersionAsync(accessToken);
        await PutFeaturesAsync(accessToken, rowVersion, donationPageEnabled: true);

        var body = await (await _client.GetAsync("/api/v1/public/site")).Content.ReadFromJsonAsync<PublicSiteResponse>();

        Assert.True(body!.DonationPageEnabled);
        var account = Assert.Single(body.BankAccounts);
        Assert.Equal("TR330006100519786457841326", account.Iban);
    }

    [Fact]
    public async Task UpdateIdentity_WithLogoReferencingUnknownMediaAsset_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var rowVersion = await GetCurrentRowVersionAsync(accessToken);

        var response = await PutIdentityAsync(accessToken, rowVersion, Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSiteSettingsGroup_AsNonAdmin_ReturnsForbidden()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email, password = "Sifre123", firstName = "Test", lastName = "User", role = "Candidate" });
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Sifre123" });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var updateRequest = new HttpRequestMessage(HttpMethod.Put, "/api/v1/admin/website/settings/theme")
        {
            Content = JsonContent.Create(new UpdateSiteSettingsThemeRequest([], "#111111", null, null)),
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
        var rowVersion = await GetCurrentRowVersionAsync(accessToken);

        await PutIdentityAsync(accessToken, rowVersion, logoId);

        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/admin/website/media/{logoId}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);

        Assert.Equal(HttpStatusCode.Conflict, deleteResponse.StatusCode);
    }
}
