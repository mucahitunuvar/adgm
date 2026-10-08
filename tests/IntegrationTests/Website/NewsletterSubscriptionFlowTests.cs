using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.ConfirmNewsletterSubscription;
using GenclikMerkezi.Modules.Website.Features.CreateLegalDocument;
using GenclikMerkezi.Modules.Website.Features.CreateLegalDocumentDraft;
using GenclikMerkezi.Modules.Website.Features.GetLegalDocumentById;
using GenclikMerkezi.Modules.Website.Features.GetNewsletterSubscribers;
using GenclikMerkezi.Modules.Website.Features.GetPersonalDataAccessLog;
using GenclikMerkezi.Modules.Website.Features.GetSiteSettings;
using GenclikMerkezi.Modules.Website.Features.GetSubmissionToken;
using GenclikMerkezi.Modules.Website.Features.PublishLegalDocumentDraft;
using GenclikMerkezi.Modules.Website.Features.SubscribeToNewsletter;
using GenclikMerkezi.Modules.Website.Features.UnsubscribeFromNewsletter;
using GenclikMerkezi.Modules.Website.Features.UpdateLegalDocumentDraftBody;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsFeatures;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsNewsletter;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §14 (Faz 3 Görev 6). Shares the "one CustomWebApplicationFactory/one Sqlite database per
// class" caveat every other Website flow test class documents. FakeEmailSender backs the confirmation
// email assertions; CustomWebApplicationFactory.GetNewsletterSubscriberStateAsync bypasses the HTTP
// surface for the UnsubscribeToken, which no response ever returns (see that helper's remarks).
public class NewsletterSubscriptionFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public NewsletterSubscriptionFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<SiteSettingsResponse> GetSiteSettingsAsync(string accessToken)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/settings", accessToken));
        return (await response.Content.ReadFromJsonAsync<SiteSettingsResponse>())!;
    }

    private async Task<LegalDocumentDetailResponse> GetLegalDocumentAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/legal-documents/{id}", accessToken));
        return (await response.Content.ReadFromJsonAsync<LegalDocumentDetailResponse>())!;
    }

    private async Task<(string Key, int VersionNumber)> CreatePublishedPrivacyNoticeAsync(string accessToken)
    {
        var key = $"nl-privacy-{Guid.NewGuid():N}";
        var createResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/legal-documents", accessToken,
            new CreateLegalDocumentRequest(key, "PrivacyNotice", "Bülten Aydınlatma Metni")));
        var created = (await createResponse.Content.ReadFromJsonAsync<CreateLegalDocumentResponse>())!;

        var detail = await GetLegalDocumentAsync(accessToken, created.Id);
        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/legal-documents/{created.Id}/versions/draft", accessToken,
            new CreateLegalDocumentDraftRequest(detail.RowVersion, null)));

        detail = await GetLegalDocumentAsync(accessToken, created.Id);
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/legal-documents/{created.Id}/versions/draft/translations/tr", accessToken,
            new UpdateLegalDocumentDraftBodyRequest(detail.RowVersion, "<p>Gövde</p>")));

        detail = await GetLegalDocumentAsync(accessToken, created.Id);
        var publishResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/legal-documents/{created.Id}/versions/draft/publish", accessToken,
            new PublishLegalDocumentDraftRequest(detail.RowVersion, null)));
        Assert.Equal(HttpStatusCode.NoContent, publishResponse.StatusCode);

        detail = await GetLegalDocumentAsync(accessToken, created.Id);
        return (key, detail.EffectiveVersionNumber!.Value);
    }

    // Enables SiteSettings.NewsletterEnabled and points NewsletterPrivacyNoticeKey at a freshly
    // published PrivacyNotice document, returning the key and its effective version number.
    private async Task<(string PrivacyKey, int PrivacyVersion)> EnableNewsletterAsync(string accessToken)
    {
        var (privacyKey, privacyVersion) = await CreatePublishedPrivacyNoticeAsync(accessToken);

        var settings = await GetSiteSettingsAsync(accessToken);
        var featuresResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, "/api/v1/admin/website/settings/features", accessToken,
            new UpdateSiteSettingsFeaturesRequest(
                settings.RowVersion, settings.GlobalSearchEnabled, true, settings.PublicJobListingsEnabled, settings.DonationPageEnabled,
                settings.AllowSearchEngineIndexing)));
        Assert.Equal(HttpStatusCode.NoContent, featuresResponse.StatusCode);

        settings = await GetSiteSettingsAsync(accessToken);
        var newsletterResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, "/api/v1/admin/website/settings/newsletter", accessToken,
            new UpdateSiteSettingsNewsletterRequest(settings.RowVersion, privacyKey)));
        Assert.Equal(HttpStatusCode.NoContent, newsletterResponse.StatusCode);

        return (privacyKey, privacyVersion);
    }

    private async Task<string> FetchSubmissionTokenAndWaitAsync()
    {
        var response = await _client.GetAsync("/api/v1/public/submission-tokens");
        var token = await response.Content.ReadFromJsonAsync<GetSubmissionTokenResponse>();
        await Task.Delay(TimeSpan.FromSeconds(3.2));
        return token!.Token;
    }

    private static string ExtractConfirmationToken(string emailBody)
    {
        var match = Regex.Match(emailBody, "token=([^\"&]+)");
        Assert.True(match.Success, $"No confirmation token found in email body: {emailBody}");
        return Uri.UnescapeDataString(match.Groups[1].Value);
    }

    [Fact]
    public async Task SubscribeToNewsletter_WhenDisabled_ReturnsNotFound()
    {
        // SiteSettings is a singleton shared by every test method in this class (IClassFixture), so
        // NewsletterEnabled may already have been turned on by another test - explicitly turn it off
        // here rather than assuming the fixture's virgin default, so this test is order-independent.
        var accessToken = await LoginAsAdminAsync();
        var settings = await GetSiteSettingsAsync(accessToken);
        var disableResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, "/api/v1/admin/website/settings/features", accessToken,
            new UpdateSiteSettingsFeaturesRequest(
                settings.RowVersion, settings.GlobalSearchEnabled, false, settings.PublicJobListingsEnabled, settings.DonationPageEnabled,
                settings.AllowSearchEngineIndexing)));
        Assert.Equal(HttpStatusCode.NoContent, disableResponse.StatusCode);

        var token = await FetchSubmissionTokenAndWaitAsync();
        var request = new SubscribeToNewsletterRequest(token, "", "", $"visitor-{Guid.NewGuid():N}@example.com", "tr", 1);

        var response = await _client.PostAsJsonAsync("/api/v1/public/newsletter/subscriptions", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SubscribeToNewsletter_WithStaleAcceptedVersion_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var (privacyKey, privacyVersion) = await EnableNewsletterAsync(accessToken);

        var token = await FetchSubmissionTokenAndWaitAsync();
        var request = new SubscribeToNewsletterRequest(
            token, "", "", $"visitor-{Guid.NewGuid():N}@example.com", "tr", privacyVersion + 1);

        var response = await _client.PostAsJsonAsync("/api/v1/public/newsletter/subscriptions", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        _ = privacyKey;
    }

    [Fact]
    public async Task SubscribeToNewsletter_WithoutSubmissionToken_IsRejected()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableNewsletterAsync(accessToken);

        var request = new SubscribeToNewsletterRequest(
            "", "", "", $"visitor-{Guid.NewGuid():N}@example.com", "tr", privacyVersion);

        var response = await _client.PostAsJsonAsync("/api/v1/public/newsletter/subscriptions", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task FullDoubleOptInFlow_SubscribeConfirmListExportUnsubscribeResubscribe_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var (privacyKey, privacyVersion) = await EnableNewsletterAsync(accessToken);
        var email = $"subscriber-{Guid.NewGuid():N}@example.com";

        // Subscribe.
        var token = await FetchSubmissionTokenAndWaitAsync();
        var subscribeRequest = new SubscribeToNewsletterRequest(token, "", "", email, "tr", privacyVersion);
        var subscribeResponse = await _client.PostAsJsonAsync("/api/v1/public/newsletter/subscriptions", subscribeRequest);
        Assert.Equal(HttpStatusCode.Accepted, subscribeResponse.StatusCode);

        var confirmationEmail = Assert.Single(_factory.EmailSender.SentEmails, e => e.ToEmail == email);
        var confirmationToken = ExtractConfirmationToken(confirmationEmail.Body);

        // A second subscribe attempt for the same (still pending) email, within the resend cooldown,
        // must not leak that it is already known and must not send a second email.
        var secondToken = await FetchSubmissionTokenAndWaitAsync();
        var secondSubscribeResponse = await _client.PostAsJsonAsync(
            "/api/v1/public/newsletter/subscriptions", new SubscribeToNewsletterRequest(secondToken, "", "", email, "tr", privacyVersion));
        Assert.Equal(HttpStatusCode.Accepted, secondSubscribeResponse.StatusCode);
        Assert.Single(_factory.EmailSender.SentEmails, e => e.ToEmail == email);

        // Confirm.
        var confirmResponse = await _client.PostAsJsonAsync(
            "/api/v1/public/newsletter/confirmations", new ConfirmNewsletterSubscriptionRequest(confirmationToken));
        Assert.Equal(HttpStatusCode.NoContent, confirmResponse.StatusCode);

        // Admin list shows it Active and the read is itself logged as personal data access.
        var listResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, "/api/v1/admin/website/newsletter-subscribers", accessToken));
        var list = (await listResponse.Content.ReadFromJsonAsync<PagedResult<NewsletterSubscriberSummaryResponse>>())!;
        var listed = Assert.Single(list.Items, i => i.Email == email);
        Assert.Equal("Active", listed.Status);

        var accessLogResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, "/api/v1/admin/website/personal-data-access-log", accessToken));
        var accessLog = (await accessLogResponse.Content.ReadFromJsonAsync<PagedResult<PersonalDataAccessLogResponse>>())!;
        Assert.Contains(accessLog.Items, l => l.EntityType == "NewsletterSubscriber" && l.Action == "View");

        // Export CSV contains the confirmed subscriber with a BOM and the three documented columns.
        var exportResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, "/api/v1/admin/website/newsletter-subscribers/export", accessToken));
        Assert.Equal(HttpStatusCode.OK, exportResponse.StatusCode);
        var csvBytes = await exportResponse.Content.ReadAsByteArrayAsync();
        Assert.Equal(0xEF, csvBytes[0]);
        Assert.Equal(0xBB, csvBytes[1]);
        Assert.Equal(0xBF, csvBytes[2]);
        var csvText = System.Text.Encoding.UTF8.GetString(csvBytes);
        Assert.Contains("email,language,confirmedAtUtc", csvText);
        Assert.Contains(email, csvText);

        var afterExportLogResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, "/api/v1/admin/website/personal-data-access-log", accessToken));
        var afterExportLog = (await afterExportLogResponse.Content.ReadFromJsonAsync<PagedResult<PersonalDataAccessLogResponse>>())!;
        Assert.Contains(afterExportLog.Items, l => l.EntityType == "NewsletterSubscriber" && l.Action == "Export");

        // Unsubscribe via the subscriber's own persisted token (never returned by any HTTP response).
        var (unsubscribeToken, statusAfterConfirm) = await _factory.GetNewsletterSubscriberStateAsync(email);
        Assert.Equal(NewsletterSubscriberStatus.Active, statusAfterConfirm);

        var unsubscribeResponse = await _client.PostAsJsonAsync(
            "/api/v1/public/newsletter/unsubscriptions", new UnsubscribeFromNewsletterRequest(unsubscribeToken));
        Assert.Equal(HttpStatusCode.NoContent, unsubscribeResponse.StatusCode);

        var (_, statusAfterUnsubscribe) = await _factory.GetNewsletterSubscriberStateAsync(email);
        Assert.Equal(NewsletterSubscriberStatus.Unsubscribed, statusAfterUnsubscribe);

        // A second unsubscribe call with the same token is idempotent (a one-click link may be opened
        // twice).
        var secondUnsubscribeResponse = await _client.PostAsJsonAsync(
            "/api/v1/public/newsletter/unsubscriptions", new UnsubscribeFromNewsletterRequest(unsubscribeToken));
        Assert.Equal(HttpStatusCode.NoContent, secondUnsubscribeResponse.StatusCode);

        // Subscribing again resurrects the row back to PendingConfirmation ("kayıt sızdırmaz").
        var resubscribeToken = await FetchSubmissionTokenAndWaitAsync();
        var resubscribeResponse = await _client.PostAsJsonAsync(
            "/api/v1/public/newsletter/subscriptions",
            new SubscribeToNewsletterRequest(resubscribeToken, "", "", email, "tr", privacyVersion));
        Assert.Equal(HttpStatusCode.Accepted, resubscribeResponse.StatusCode);

        var (_, statusAfterResubscribe) = await _factory.GetNewsletterSubscriberStateAsync(email);
        Assert.Equal(NewsletterSubscriberStatus.PendingConfirmation, statusAfterResubscribe);
        Assert.Equal(2, _factory.EmailSender.SentEmails.Count(e => e.ToEmail == email));

        _ = privacyKey;
    }

    [Fact]
    public async Task ConfirmNewsletterSubscription_WithTamperedToken_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/public/newsletter/confirmations", new ConfirmNewsletterSubscriptionRequest("not-a-real-token"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UnsubscribeFromNewsletter_WithUnknownToken_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/public/newsletter/unsubscriptions", new UnsubscribeFromNewsletterRequest("unknown-token"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteNewsletterSubscriber_RemovesItFromTheAdminList()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableNewsletterAsync(accessToken);
        var email = $"to-delete-{Guid.NewGuid():N}@example.com";

        var token = await FetchSubmissionTokenAndWaitAsync();
        await _client.PostAsJsonAsync(
            "/api/v1/public/newsletter/subscriptions", new SubscribeToNewsletterRequest(token, "", "", email, "tr", privacyVersion));

        var listResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, "/api/v1/admin/website/newsletter-subscribers", accessToken));
        var list = (await listResponse.Content.ReadFromJsonAsync<PagedResult<NewsletterSubscriberSummaryResponse>>())!;
        var subscriberId = Assert.Single(list.Items, i => i.Email == email).Id;

        var deleteResponse = await _client.SendAsync(Authorized(
            HttpMethod.Delete, $"/api/v1/admin/website/newsletter-subscribers/{subscriberId}", accessToken));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDeleteResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, "/api/v1/admin/website/newsletter-subscribers", accessToken));
        var afterDeleteList = (await afterDeleteResponse.Content.ReadFromJsonAsync<PagedResult<NewsletterSubscriberSummaryResponse>>())!;
        Assert.DoesNotContain(afterDeleteList.Items, i => i.Id == subscriberId);

        // Deleting an already-deleted (or unknown) id is reported, not silently accepted.
        var secondDeleteResponse = await _client.SendAsync(Authorized(
            HttpMethod.Delete, $"/api/v1/admin/website/newsletter-subscribers/{subscriberId}", accessToken));
        Assert.Equal(HttpStatusCode.NotFound, secondDeleteResponse.StatusCode);
    }
}
