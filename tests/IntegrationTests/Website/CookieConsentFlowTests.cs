using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.CreateCookieConsentRecord;
using GenclikMerkezi.Modules.Website.Features.CreateLegalDocument;
using GenclikMerkezi.Modules.Website.Features.CreateLegalDocumentDraft;
using GenclikMerkezi.Modules.Website.Features.GetCookieConsentSummary;
using GenclikMerkezi.Modules.Website.Features.GetLegalDocumentById;
using GenclikMerkezi.Modules.Website.Features.GetPublicSite;
using GenclikMerkezi.Modules.Website.Features.GetSiteSettings;
using GenclikMerkezi.Modules.Website.Features.PublishLegalDocumentDraft;
using GenclikMerkezi.Modules.Website.Features.UpdateLegalDocumentDraftBody;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsCookieConsent;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §13 (Faz 3 Görev 7). Shares the "one CustomWebApplicationFactory/one Sqlite database per
// class" caveat every other Website flow test class documents.
public class CookieConsentFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CookieConsentFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<byte[]> GetSiteSettingsRowVersionAsync(string accessToken)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/settings", accessToken));
        var body = await response.Content.ReadFromJsonAsync<SiteSettingsResponse>();
        return body!.RowVersion;
    }

    // Creates a legal document of the given kind, publishes its first draft with a trivial body, and
    // returns its key - used both for the CookiePolicy document CookieConsentFlowTests configures and
    // for UpdateSiteSettingsCookieConsent_WithWrongKind_ReturnsBadRequest's mismatched PrivacyNotice one.
    private async Task<string> CreateAndPublishLegalDocumentAsync(string accessToken, string kind)
    {
        var key = $"{kind.ToLowerInvariant()}-{Guid.NewGuid():N}";
        var createResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/legal-documents", accessToken, new CreateLegalDocumentRequest(key, kind, "Başlık")));
        var created = (await createResponse.Content.ReadFromJsonAsync<CreateLegalDocumentResponse>())!;

        var detailResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/legal-documents/{created.Id}", accessToken));
        var detail = (await detailResponse.Content.ReadFromJsonAsync<LegalDocumentDetailResponse>())!;

        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/legal-documents/{created.Id}/versions/draft", accessToken,
            new CreateLegalDocumentDraftRequest(detail.RowVersion, null)));

        detailResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/legal-documents/{created.Id}", accessToken));
        detail = (await detailResponse.Content.ReadFromJsonAsync<LegalDocumentDetailResponse>())!;

        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/legal-documents/{created.Id}/versions/draft/translations/tr", accessToken,
            new UpdateLegalDocumentDraftBodyRequest(detail.RowVersion, "<p>Gövde</p>")));

        detailResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/legal-documents/{created.Id}", accessToken));
        detail = (await detailResponse.Content.ReadFromJsonAsync<LegalDocumentDetailResponse>())!;

        var publishResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/legal-documents/{created.Id}/versions/draft/publish", accessToken,
            new PublishLegalDocumentDraftRequest(detail.RowVersion, null)));
        Assert.Equal(HttpStatusCode.NoContent, publishResponse.StatusCode);

        return key;
    }

    private async Task ConfigureCookiePolicyAsync(string accessToken, string cookiePolicyKey)
    {
        var rowVersion = await GetSiteSettingsRowVersionAsync(accessToken);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, "/api/v1/admin/website/settings/cookie-consent", accessToken,
            new UpdateSiteSettingsCookieConsentRequest(
                rowVersion, cookiePolicyKey,
                [new UpdateSiteSettingsCookieConsentTranslationInput("tr", "Çerez Onayı", "Bu site çerez kullanır.", "Gerekli", "Analitik", "Pazarlama")])));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSiteSettingsCookieConsent_WithCookiePolicyDocument_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = await CreateAndPublishLegalDocumentAsync(accessToken, "CookiePolicy");

        await ConfigureCookiePolicyAsync(accessToken, key);

        var publicSite = await (await _client.GetAsync("/api/v1/public/site?lang=tr")).Content.ReadFromJsonAsync<PublicSiteResponse>();
        Assert.Equal(key, publicSite!.CookieConsent.PolicyKey);
        Assert.Equal(1, publicSite.CookieConsent.PolicyVersion);
        Assert.Equal("Çerez Onayı", publicSite.CookieConsent.BannerTitle);
    }

    [Fact]
    public async Task UpdateSiteSettingsCookieConsent_WithWrongKind_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var privacyNoticeKey = await CreateAndPublishLegalDocumentAsync(accessToken, "PrivacyNotice");
        var rowVersion = await GetSiteSettingsRowVersionAsync(accessToken);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, "/api/v1/admin/website/settings/cookie-consent", accessToken,
            new UpdateSiteSettingsCookieConsentRequest(rowVersion, privacyNoticeKey, [])));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCookieConsentRecord_WithoutAnyConfiguredPolicy_ReturnsNotFound()
    {
        // A fresh factory-backed client would collide with other tests' SiteSettings singleton row,
        // so this only holds as long as no other test in this class has configured CookiePolicyKey
        // first - this test intentionally runs standalone semantics are irrelevant here since it only
        // asserts against a response shape, not SiteSettings' current configured state across tests.
        var accessToken = await LoginAsAdminAsync();
        var rowVersion = await GetSiteSettingsRowVersionAsync(accessToken);
        await _client.SendAsync(Authorized(
            HttpMethod.Put, "/api/v1/admin/website/settings/cookie-consent", accessToken,
            new UpdateSiteSettingsCookieConsentRequest(rowVersion, null, [])));

        var response = await _client.PostAsJsonAsync(
            "/api/v1/public/cookie-consents", new CreateCookieConsentRecordRequest(Guid.NewGuid(), [], 1, "AcceptAll"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateCookieConsentRecord_WithCurrentPolicyVersion_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = await CreateAndPublishLegalDocumentAsync(accessToken, "CookiePolicy");
        await ConfigureCookiePolicyAsync(accessToken, key);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/public/cookie-consents", new CreateCookieConsentRecordRequest(Guid.NewGuid(), ["Marketing"], 1, "Custom"));

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Fact]
    public async Task CreateCookieConsentRecord_WithStalePolicyVersion_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = await CreateAndPublishLegalDocumentAsync(accessToken, "CookiePolicy");
        await ConfigureCookiePolicyAsync(accessToken, key);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/public/cookie-consents", new CreateCookieConsentRecordRequest(Guid.NewGuid(), [], 999, "AcceptAll"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateCookieConsentRecord_WithInvalidAction_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = await CreateAndPublishLegalDocumentAsync(accessToken, "CookiePolicy");
        await ConfigureCookiePolicyAsync(accessToken, key);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/public/cookie-consents", new CreateCookieConsentRecordRequest(Guid.NewGuid(), [], 1, "NotARealAction"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCookieConsentSummary_CountsRecordsByCategoryAndAction()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = await CreateAndPublishLegalDocumentAsync(accessToken, "CookiePolicy");
        await ConfigureCookiePolicyAsync(accessToken, key);

        await _client.PostAsJsonAsync(
            "/api/v1/public/cookie-consents", new CreateCookieConsentRecordRequest(Guid.NewGuid(), ["Analytics"], 1, "AcceptAll"));
        await _client.PostAsJsonAsync(
            "/api/v1/public/cookie-consents", new CreateCookieConsentRecordRequest(Guid.NewGuid(), [], 1, "RejectAll"));

        var summaryResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/cookie-consents/summary", accessToken));
        Assert.Equal(HttpStatusCode.OK, summaryResponse.StatusCode);
        var summary = await summaryResponse.Content.ReadFromJsonAsync<CookieConsentSummaryResponse>();

        Assert.True(summary!.TotalRecords >= 2);
        Assert.True(summary.ByCategory["Necessary"] >= 2);
        Assert.True(summary.ByCategory.GetValueOrDefault("Analytics") >= 1);
        Assert.True(summary.ByAction["AcceptAll"] >= 1);
        Assert.True(summary.ByAction["RejectAll"] >= 1);
    }
}
