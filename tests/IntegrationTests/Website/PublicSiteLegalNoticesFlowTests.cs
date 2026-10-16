using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.CreateLegalDocument;
using GenclikMerkezi.Modules.Website.Features.CreateLegalDocumentDraft;
using GenclikMerkezi.Modules.Website.Features.GetLegalDocumentById;
using GenclikMerkezi.Modules.Website.Features.GetPublicSite;
using GenclikMerkezi.Modules.Website.Features.GetSiteSettings;
using GenclikMerkezi.Modules.Website.Features.PublishLegalDocumentDraft;
using GenclikMerkezi.Modules.Website.Features.UpdateLegalDocumentDraftBody;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsEvents;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsNewsletter;

namespace GenclikMerkezi.IntegrationTests.Website;

// Madde 36/38 (Faz 3 Koşu 2C/3B): GET /public/site now carries newsletterPrivacyNotice/
// eventPrivacyNotice {key, version} - the same resolution CookieConsentFlowTests already exercises
// for CookiePolicyKey, reused via GetPublicSiteQueryHandler.ResolveLegalNoticeAsync.
public class PublicSiteLegalNoticesFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PublicSiteLegalNoticesFlowTests(CustomWebApplicationFactory factory)
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

    // Copy of CookieConsentFlowTests' own helper (same "one HTTP-helper copy per flow test class"
    // precedent PublicJobsFlowTests documents) - creates a PrivacyNotice document, publishes its first
    // version with a trivial body, and returns its key + the admin legal-document id (needed here to
    // publish a SECOND version for the "new version updates public/site" scenario).
    private async Task<(string Key, Guid DocumentId)> CreateAndPublishLegalDocumentAsync(string accessToken)
    {
        var key = $"privacynotice-{Guid.NewGuid():N}";
        var createResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/legal-documents", accessToken,
            new CreateLegalDocumentRequest(key, "PrivacyNotice", "Başlık")));
        var created = (await createResponse.Content.ReadFromJsonAsync<CreateLegalDocumentResponse>())!;

        await PublishNewDraftVersionAsync(accessToken, created.Id);

        return (key, created.Id);
    }

    private async Task PublishNewDraftVersionAsync(string accessToken, Guid documentId)
    {
        var detailResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/legal-documents/{documentId}", accessToken));
        var detail = (await detailResponse.Content.ReadFromJsonAsync<LegalDocumentDetailResponse>())!;

        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/legal-documents/{documentId}/versions/draft", accessToken,
            new CreateLegalDocumentDraftRequest(detail.RowVersion, null)));

        detailResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/legal-documents/{documentId}", accessToken));
        detail = (await detailResponse.Content.ReadFromJsonAsync<LegalDocumentDetailResponse>())!;

        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/legal-documents/{documentId}/versions/draft/translations/tr", accessToken,
            new UpdateLegalDocumentDraftBodyRequest(detail.RowVersion, "<p>Gövde</p>")));

        detailResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/legal-documents/{documentId}", accessToken));
        detail = (await detailResponse.Content.ReadFromJsonAsync<LegalDocumentDetailResponse>())!;

        var publishResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/legal-documents/{documentId}/versions/draft/publish", accessToken,
            new PublishLegalDocumentDraftRequest(detail.RowVersion, null)));
        Assert.Equal(HttpStatusCode.NoContent, publishResponse.StatusCode);
    }

    private async Task ConfigureNewsletterPrivacyNoticeAsync(string accessToken, string? key)
    {
        var rowVersion = await GetSiteSettingsRowVersionAsync(accessToken);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, "/api/v1/admin/website/settings/newsletter", accessToken,
            new UpdateSiteSettingsNewsletterRequest(rowVersion, key)));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task ConfigureEventPrivacyNoticeAsync(string accessToken, string? key)
    {
        var rowVersion = await GetSiteSettingsRowVersionAsync(accessToken);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, "/api/v1/admin/website/settings/events", accessToken,
            new UpdateSiteSettingsEventsRequest(rowVersion, key)));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<PublicSiteResponse> GetPublicSiteAsync() =>
        (await (await _client.GetAsync("/api/v1/public/site?lang=tr")).Content.ReadFromJsonAsync<PublicSiteResponse>())!;

    [Fact]
    public async Task GetPublicSite_WithoutNewsletterOrEventPrivacyNoticeKeyConfigured_ReturnsNullForBoth()
    {
        var accessToken = await LoginAsAdminAsync();
        await ConfigureNewsletterPrivacyNoticeAsync(accessToken, null);
        await ConfigureEventPrivacyNoticeAsync(accessToken, null);

        var publicSite = await GetPublicSiteAsync();

        Assert.Null(publicSite.NewsletterPrivacyNotice);
        Assert.Null(publicSite.EventPrivacyNotice);
    }

    [Fact]
    public async Task GetPublicSite_WithNewsletterPrivacyNoticeKeyConfigured_ReturnsKeyAndVersion()
    {
        var accessToken = await LoginAsAdminAsync();
        var (key, _) = await CreateAndPublishLegalDocumentAsync(accessToken);
        await ConfigureNewsletterPrivacyNoticeAsync(accessToken, key);

        var publicSite = await GetPublicSiteAsync();

        Assert.NotNull(publicSite.NewsletterPrivacyNotice);
        Assert.Equal(key, publicSite.NewsletterPrivacyNotice!.Key);
        Assert.Equal(1, publicSite.NewsletterPrivacyNotice.Version);
    }

    [Fact]
    public async Task GetPublicSite_WithEventPrivacyNoticeKeyConfigured_ReturnsKeyAndVersion()
    {
        var accessToken = await LoginAsAdminAsync();
        var (key, _) = await CreateAndPublishLegalDocumentAsync(accessToken);
        await ConfigureEventPrivacyNoticeAsync(accessToken, key);

        var publicSite = await GetPublicSiteAsync();

        Assert.NotNull(publicSite.EventPrivacyNotice);
        Assert.Equal(key, publicSite.EventPrivacyNotice!.Key);
        Assert.Equal(1, publicSite.EventPrivacyNotice.Version);
    }

    [Fact]
    public async Task GetPublicSite_AfterPublishingNewLegalDocumentVersion_NewsletterPrivacyNoticeVersionUpdates()
    {
        var accessToken = await LoginAsAdminAsync();
        var (key, documentId) = await CreateAndPublishLegalDocumentAsync(accessToken);
        await ConfigureNewsletterPrivacyNoticeAsync(accessToken, key);

        var beforeSecondVersion = await GetPublicSiteAsync();
        Assert.Equal(1, beforeSecondVersion.NewsletterPrivacyNotice!.Version);

        await PublishNewDraftVersionAsync(accessToken, documentId);

        var afterSecondVersion = await GetPublicSiteAsync();
        Assert.Equal(key, afterSecondVersion.NewsletterPrivacyNotice!.Key);
        Assert.Equal(2, afterSecondVersion.NewsletterPrivacyNotice.Version);
    }
}
