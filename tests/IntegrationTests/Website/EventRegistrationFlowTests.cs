using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.CancelEventRegistration;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.CreateEventRegistration;
using GenclikMerkezi.Modules.Website.Features.CreateLegalDocument;
using GenclikMerkezi.Modules.Website.Features.CreateLegalDocumentDraft;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.GetLegalDocumentById;
using GenclikMerkezi.Modules.Website.Features.GetSiteSettings;
using GenclikMerkezi.Modules.Website.Features.GetSubmissionToken;
using GenclikMerkezi.Modules.Website.Features.PublishContentItem;
using GenclikMerkezi.Modules.Website.Features.PublishLegalDocumentDraft;
using GenclikMerkezi.Modules.Website.Features.ResendEventRegistrationVerification;
using GenclikMerkezi.Modules.Website.Features.UpdateLegalDocumentDraftBody;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsEvents;
using GenclikMerkezi.Modules.Website.Features.UpsertEventSchedule;
using GenclikMerkezi.Modules.Website.Features.VerifyEventRegistration;
using GenclikMerkezi.Modules.Website.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §11.2 (Faz 4 Görev 3). Shares the "one CustomWebApplicationFactory/one Sqlite database per
// class" caveat every other Website flow test class documents. Uses the seeded "event" ContentType
// (SupportsEvent) from GenclikMerkeziContentTypeSeed, one freshly created+published ContentItem per
// test so capacity/counter assertions never collide with another test's event.
public class EventRegistrationFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly CreateContentItemSeoInput EmptySeo = new(null, null, null, null, null, null, null, false);

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EventRegistrationFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(string Email, string AccessToken, Guid UserId)> RegisterVerifiedUserAsync()
    {
        var email = $"verified-{Guid.NewGuid():N}@example.com";
        const string password = "KullaniciSifre123";
        var userId = await _factory.SeedAdminUserAsync(email, password);

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return (email, login!.AccessToken, userId);
    }

    private async Task<string> LoginAsAdminAsync()
    {
        var (_, accessToken, _) = await RegisterVerifiedUserAsync();
        return accessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string accessToken, object? body = null)
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
        var key = $"event-privacy-{Guid.NewGuid():N}";
        var createResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/legal-documents", accessToken,
            new CreateLegalDocumentRequest(key, "PrivacyNotice", "Etkinlik Aydınlatma Metni")));
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

    // Points SiteSettings.EventPrivacyNoticeKey at a freshly published PrivacyNotice document.
    private async Task<(string PrivacyKey, int PrivacyVersion)> EnableEventRegistrationAsync(string accessToken)
    {
        var (privacyKey, privacyVersion) = await CreatePublishedPrivacyNoticeAsync(accessToken);

        var settings = await GetSiteSettingsAsync(accessToken);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, "/api/v1/admin/website/settings/events", accessToken,
            new UpdateSiteSettingsEventsRequest(settings.RowVersion, privacyKey)));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        return (privacyKey, privacyVersion);
    }

    private async Task<Guid> GetContentTypeIdAsync(string accessToken, string key)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/content-types", accessToken));
        var types = await response.Content.ReadFromJsonAsync<List<ContentTypeSummaryResponse>>();
        return types!.Single(t => t.Key == key).Id;
    }

    private async Task<Guid> CreateContentItemAsync(string accessToken, Guid contentTypeId, string title)
    {
        var request = new CreateContentItemRequest(contentTypeId, null, 1, false, null, null, title, null, null, null, EmptySeo);
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, request));
        var created = await response.Content.ReadFromJsonAsync<CreateContentItemResponse>();
        return created!.Id;
    }

    private async Task PublishAsync(string accessToken, Guid contentItemId)
    {
        var detailResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{contentItemId}", accessToken));
        var detail = await detailResponse.Content
            .ReadFromJsonAsync<GenclikMerkezi.Modules.Website.Features.GetContentItemById.ContentItemDetailResponse>();
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{contentItemId}/publish", accessToken,
            new PublishContentItemRequest(detail!.RowVersion, null, null)));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private static UpsertEventScheduleRequest NewScheduleRequest(
        DateTime? startsAtUtc = null,
        int? capacity = 10,
        bool autoConfirm = true,
        bool waitlistEnabled = true,
        string format = "InPerson",
        string? onlineLink = null) =>
        new(
            null, startsAtUtc ?? DateTime.UtcNow.AddDays(10), (startsAtUtc ?? DateTime.UtcNow.AddDays(10)).AddHours(2), format, onlineLink,
            capacity, true, null, null, null, null, autoConfirm, waitlistEnabled,
            [new UpsertEventScheduleTranslationInput("tr", "Gençlik Merkezi Salonu", "Örnek Adres No:1", "Ücretsiz", "Eğitmen", "<p>Program</p>", "Not")]);

    private async Task UpsertScheduleAsync(string accessToken, Guid contentItemId, UpsertEventScheduleRequest request)
    {
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken, request));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<Guid> CreatePublishedEventAsync(string accessToken, Guid contentTypeId, UpsertEventScheduleRequest scheduleRequest)
    {
        var id = await CreateContentItemAsync(accessToken, contentTypeId, $"Etkinlik-{Guid.NewGuid():N}");
        await PublishAsync(accessToken, id);
        await UpsertScheduleAsync(accessToken, id, scheduleRequest);
        return id;
    }

    private async Task<string> FetchSubmissionTokenAndWaitAsync()
    {
        var response = await _client.GetAsync("/api/v1/public/submission-tokens");
        var token = await response.Content.ReadFromJsonAsync<GetSubmissionTokenResponse>();
        await Task.Delay(TimeSpan.FromSeconds(3.2));
        return token!.Token;
    }

    private static string ExtractQueryParam(string body, string paramName)
    {
        var match = Regex.Match(body, $"{paramName}=([^\"&]+)");
        Assert.True(match.Success, $"No '{paramName}' found in body: {body}");
        return Uri.UnescapeDataString(match.Groups[1].Value);
    }

    private async Task<HttpResponseMessage> CreateAnonymousRegistrationAsync(
        Guid contentItemId, string email, int privacyVersion, string submissionToken) =>
        await _client.PostAsJsonAsync(
            $"/api/v1/public/events/{contentItemId}/registrations",
            new CreateEventRegistrationRequest(submissionToken, "", "", "Ahmet", "Yılmaz", email, null, "tr", privacyVersion));

    private async Task<HttpResponseMessage> CreateAuthenticatedRegistrationAsync(
        Guid contentItemId, string email, int privacyVersion, string accessToken) =>
        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/public/events/{contentItemId}/registrations", accessToken,
            new CreateEventRegistrationRequest("", "", "", "Ahmet", "Yılmaz", email, null, "tr", privacyVersion)));

    // ADR-024 §11.2 Faz 4 Görev 3 test bypass: no admin listing endpoint exists yet for the
    // verification-expiry/cooldown fields, so these two helpers reach into WebsiteDbContext directly -
    // mirrors CustomWebApplicationFactory's own GetEventRegistrationSnapshotAsync bypass.
    private async Task ExpireVerificationTokenAsync(Guid registrationId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WebsiteDbContext>();
        var registration = await dbContext.EventRegistrations.FirstAsync(r => r.Id == registrationId);
        dbContext.Entry(registration).Property("VerificationTokenExpiresAtUtc").CurrentValue = DateTime.UtcNow.AddHours(-1);
        await dbContext.SaveChangesAsync();
    }

    private async Task RewindVerificationCooldownAsync(Guid registrationId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WebsiteDbContext>();
        var registration = await dbContext.EventRegistrations.FirstAsync(r => r.Id == registrationId);
        dbContext.Entry(registration).Property("LastVerificationEmailSentAtUtc").CurrentValue = (DateTime?)null;
        await dbContext.SaveChangesAsync();
    }

    // --- Capacity decision table (logged-in path - decided immediately, no guard wait needed) ---

    [Fact]
    public async Task CreateEventRegistration_AutoConfirmTrue_CapacityAvailable_BecomesConfirmed()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(capacity: 10, autoConfirm: true, waitlistEnabled: true));

        var (email, userAccessToken, _) = await RegisterVerifiedUserAsync();
        var response = await CreateAuthenticatedRegistrationAsync(contentItemId, email, privacyVersion, userAccessToken);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var snapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, email);
        Assert.Equal(EventRegistrationStatus.Confirmed, snapshot!.Status);
        var (confirmedCount, waitlistedCount) = await _factory.GetEventScheduleCountersAsync(contentItemId);
        Assert.Equal(1, confirmedCount);
        Assert.Equal(0, waitlistedCount);
    }

    [Fact]
    public async Task CreateEventRegistration_AutoConfirmFalse_CapacityAvailable_BecomesApplied()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(capacity: 10, autoConfirm: false, waitlistEnabled: true));

        var (email, userAccessToken, _) = await RegisterVerifiedUserAsync();
        var response = await CreateAuthenticatedRegistrationAsync(contentItemId, email, privacyVersion, userAccessToken);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var snapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, email);
        Assert.Equal(EventRegistrationStatus.Applied, snapshot!.Status);
        var (confirmedCount, waitlistedCount) = await _factory.GetEventScheduleCountersAsync(contentItemId);
        Assert.Equal(0, confirmedCount);
        Assert.Equal(0, waitlistedCount);
    }

    [Fact]
    public async Task CreateEventRegistration_CapacityFull_WaitlistEnabled_BecomesWaitlisted()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(capacity: 1, autoConfirm: true, waitlistEnabled: true));

        var (firstEmail, firstAccessToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, firstEmail, privacyVersion, firstAccessToken);

        var (secondEmail, secondAccessToken, _) = await RegisterVerifiedUserAsync();
        var response = await CreateAuthenticatedRegistrationAsync(contentItemId, secondEmail, privacyVersion, secondAccessToken);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var snapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, secondEmail);
        Assert.Equal(EventRegistrationStatus.Waitlisted, snapshot!.Status);
        var (confirmedCount, waitlistedCount) = await _factory.GetEventScheduleCountersAsync(contentItemId);
        Assert.Equal(1, confirmedCount);
        Assert.Equal(1, waitlistedCount);
    }

    [Fact]
    public async Task CreateEventRegistration_CapacityFull_WaitlistDisabled_ReturnsConflict_AndPersistsNothing()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(capacity: 1, autoConfirm: true, waitlistEnabled: false));

        var (firstEmail, firstAccessToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, firstEmail, privacyVersion, firstAccessToken);

        var (secondEmail, secondAccessToken, _) = await RegisterVerifiedUserAsync();
        var response = await CreateAuthenticatedRegistrationAsync(contentItemId, secondEmail, privacyVersion, secondAccessToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var snapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, secondEmail);
        Assert.Null(snapshot);
        var (confirmedCount, waitlistedCount) = await _factory.GetEventScheduleCountersAsync(contentItemId);
        Assert.Equal(1, confirmedCount);
        Assert.Equal(0, waitlistedCount);
    }

    // --- Anonymous flow: PendingVerification, duplicate handling, UserId never from client ---

    [Fact]
    public async Task CreateEventRegistration_Anonymous_StartsPendingVerification_AndDoesNotHoldCapacity()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(capacity: 1, autoConfirm: true, waitlistEnabled: true));
        var email = $"anon-{Guid.NewGuid():N}@example.com";

        var token = await FetchSubmissionTokenAndWaitAsync();
        var response = await CreateAnonymousRegistrationAsync(contentItemId, email, privacyVersion, token);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var snapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, email);
        Assert.Equal(EventRegistrationStatus.PendingVerification, snapshot!.Status);
        Assert.Null(snapshot.UserId);
        var (confirmedCount, _) = await _factory.GetEventScheduleCountersAsync(contentItemId);
        Assert.Equal(0, confirmedCount);
    }

    [Fact]
    public async Task CreateEventRegistration_LoggedIn_PersistsUserIdFromToken_NotFromRequestBody()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(accessToken, eventTypeId, NewScheduleRequest());

        var (email, userAccessToken, userId) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, email, privacyVersion, userAccessToken);

        var snapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, email);
        Assert.Equal(userId, snapshot!.UserId);
    }

    [Fact]
    public async Task CreateEventRegistration_LoggedIn_Duplicate_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(accessToken, eventTypeId, NewScheduleRequest());

        var (email, userAccessToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, email, privacyVersion, userAccessToken);

        var secondResponse = await CreateAuthenticatedRegistrationAsync(contentItemId, email, privacyVersion, userAccessToken);

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task CreateEventRegistration_WithStaleAcceptedPrivacyVersion_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(accessToken, eventTypeId, NewScheduleRequest());

        var (email, userAccessToken, _) = await RegisterVerifiedUserAsync();
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/public/events/{contentItemId}/registrations", userAccessToken,
            new CreateEventRegistrationRequest("", "", "", "Ahmet", "Yılmaz", email, null, "tr", privacyVersion + 1)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateEventRegistration_Duplicate_AnonymousStillPending_ReturnsUniform202_WithoutCreatingASecondRow()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(accessToken, eventTypeId, NewScheduleRequest());
        var email = $"dup-pending-{Guid.NewGuid():N}@example.com";

        var firstToken = await FetchSubmissionTokenAndWaitAsync();
        await CreateAnonymousRegistrationAsync(contentItemId, email, privacyVersion, firstToken);
        var firstSnapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, email);

        var secondToken = await FetchSubmissionTokenAndWaitAsync();
        var secondResponse = await CreateAnonymousRegistrationAsync(contentItemId, email, privacyVersion, secondToken);

        Assert.Equal(HttpStatusCode.Accepted, secondResponse.StatusCode);
        var secondSnapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, email);
        Assert.Equal(firstSnapshot!.Id, secondSnapshot!.Id);
    }

    [Fact]
    public async Task CreateEventRegistration_Duplicate_AnonymousAlreadyConfirmed_SendsAlreadyRegisteredNotice()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(accessToken, eventTypeId, NewScheduleRequest());
        var email = $"dup-confirmed-{Guid.NewGuid():N}@example.com";

        var token = await FetchSubmissionTokenAndWaitAsync();
        await CreateAnonymousRegistrationAsync(contentItemId, email, privacyVersion, token);
        var verificationEmail = Assert.Single(_factory.EmailSender.SentEmails, e => e.ToEmail == email);
        var verificationToken = ExtractQueryParam(verificationEmail.Body, "token");
        await _client.PostAsJsonAsync(
            "/api/v1/public/event-registrations/verifications", new VerifyEventRegistrationRequest(verificationToken));

        var secondToken = await FetchSubmissionTokenAndWaitAsync();
        var secondResponse = await CreateAnonymousRegistrationAsync(contentItemId, email, privacyVersion, secondToken);

        Assert.Equal(HttpStatusCode.Accepted, secondResponse.StatusCode);
        // Verification email + the Confirmed decision email it triggered + the duplicate POST's
        // "already registered" notice.
        Assert.Equal(3, _factory.EmailSender.SentEmails.Count(e => e.ToEmail == email));
        var snapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, email);
        Assert.Equal(EventRegistrationStatus.Confirmed, snapshot!.Status);
    }

    // --- Verification token: tampering, expiry, idempotency, email content ---

    [Fact]
    public async Task VerifyEventRegistration_WithTamperedToken_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/public/event-registrations/verifications", new VerifyEventRegistrationRequest("not-a-real-token"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task VerifyEventRegistration_WithExpiredToken_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(accessToken, eventTypeId, NewScheduleRequest());
        var email = $"expired-{Guid.NewGuid():N}@example.com";

        var token = await FetchSubmissionTokenAndWaitAsync();
        await CreateAnonymousRegistrationAsync(contentItemId, email, privacyVersion, token);
        var verificationEmail = Assert.Single(_factory.EmailSender.SentEmails, e => e.ToEmail == email);
        var verificationToken = ExtractQueryParam(verificationEmail.Body, "token");
        var snapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, email);
        await ExpireVerificationTokenAsync(snapshot!.Id);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/public/event-registrations/verifications", new VerifyEventRegistrationRequest(verificationToken));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task VerifyEventRegistration_CalledTwice_SecondCallIsIdempotent()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(capacity: 10, autoConfirm: true, waitlistEnabled: true));
        var email = $"twice-{Guid.NewGuid():N}@example.com";

        var token = await FetchSubmissionTokenAndWaitAsync();
        await CreateAnonymousRegistrationAsync(contentItemId, email, privacyVersion, token);
        var verificationEmail = Assert.Single(_factory.EmailSender.SentEmails, e => e.ToEmail == email);
        var verificationToken = ExtractQueryParam(verificationEmail.Body, "token");

        var firstResponse = await _client.PostAsJsonAsync(
            "/api/v1/public/event-registrations/verifications", new VerifyEventRegistrationRequest(verificationToken));
        var secondResponse = await _client.PostAsJsonAsync(
            "/api/v1/public/event-registrations/verifications", new VerifyEventRegistrationRequest(verificationToken));

        Assert.Equal(HttpStatusCode.NoContent, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, secondResponse.StatusCode);
        var (confirmedCount, _) = await _factory.GetEventScheduleCountersAsync(contentItemId);
        Assert.Equal(1, confirmedCount);
    }

    [Fact]
    public async Task VerifyEventRegistration_Confirmed_SendsEmailWithIcsAndCancelLink_ButNeverOnlineLinkForInPersonEvent()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(format: "InPerson", onlineLink: null));
        var email = $"confirm-email-{Guid.NewGuid():N}@example.com";

        var token = await FetchSubmissionTokenAndWaitAsync();
        await CreateAnonymousRegistrationAsync(contentItemId, email, privacyVersion, token);
        var verificationEmail = Assert.Single(_factory.EmailSender.SentEmails, e => e.ToEmail == email);
        var verificationToken = ExtractQueryParam(verificationEmail.Body, "token");
        await _client.PostAsJsonAsync(
            "/api/v1/public/event-registrations/verifications", new VerifyEventRegistrationRequest(verificationToken));

        var decisionEmail = Assert.Single(
            _factory.EmailSender.SentEmails, e => e.ToEmail == email && e.Body != verificationEmail.Body);
        Assert.Contains("calendar.ics", decisionEmail.Body, StringComparison.Ordinal);
        Assert.Contains("/events/registrations/cancel?token=", decisionEmail.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("https://meet.example.com", decisionEmail.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task VerifyEventRegistration_Confirmed_ForOnlineEvent_IncludesOnlineLink()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(format: "Online", onlineLink: "https://meet.example.com/etkinlik"));
        var email = $"online-confirm-{Guid.NewGuid():N}@example.com";

        var token = await FetchSubmissionTokenAndWaitAsync();
        await CreateAnonymousRegistrationAsync(contentItemId, email, privacyVersion, token);
        var verificationEmail = Assert.Single(_factory.EmailSender.SentEmails, e => e.ToEmail == email);
        var verificationToken = ExtractQueryParam(verificationEmail.Body, "token");
        await _client.PostAsJsonAsync(
            "/api/v1/public/event-registrations/verifications", new VerifyEventRegistrationRequest(verificationToken));

        var decisionEmail = Assert.Single(
            _factory.EmailSender.SentEmails, e => e.ToEmail == email && e.Body != verificationEmail.Body);
        Assert.Contains("https://meet.example.com/etkinlik", decisionEmail.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("https://meet.example.com/etkinlik", verificationEmail.Body, StringComparison.Ordinal);
    }

    // --- Resend verification ---

    [Fact]
    public async Task ResendEventRegistrationVerification_ForUnknownRegistration_ReturnsUniform202()
    {
        var submissionToken = await FetchSubmissionTokenAndWaitAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/v1/public/event-registrations/verification-resends",
            new ResendEventRegistrationVerificationRequest(Guid.NewGuid(), submissionToken, "", "", "unknown@example.com"));

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Fact]
    public async Task ResendEventRegistrationVerification_AfterCooldownElapsed_IssuesNewTokenAndInvalidatesOld()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(accessToken, eventTypeId, NewScheduleRequest());
        var email = $"resend-{Guid.NewGuid():N}@example.com";

        var token = await FetchSubmissionTokenAndWaitAsync();
        await CreateAnonymousRegistrationAsync(contentItemId, email, privacyVersion, token);
        var firstEmail = Assert.Single(_factory.EmailSender.SentEmails, e => e.ToEmail == email);
        var firstToken = ExtractQueryParam(firstEmail.Body, "token");
        var snapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, email);
        await RewindVerificationCooldownAsync(snapshot!.Id);
        var resendSubmissionToken = await FetchSubmissionTokenAndWaitAsync();

        var resendResponse = await _client.PostAsJsonAsync(
            "/api/v1/public/event-registrations/verification-resends",
            new ResendEventRegistrationVerificationRequest(contentItemId, resendSubmissionToken, "", "", email));

        Assert.Equal(HttpStatusCode.Accepted, resendResponse.StatusCode);
        Assert.Equal(2, _factory.EmailSender.SentEmails.Count(e => e.ToEmail == email));
        var secondEmail = Assert.Single(
            _factory.EmailSender.SentEmails, e => e.ToEmail == email && e.Body != firstEmail.Body);
        var secondToken = ExtractQueryParam(secondEmail.Body, "token");
        Assert.NotEqual(firstToken, secondToken);

        var oldTokenResponse = await _client.PostAsJsonAsync(
            "/api/v1/public/event-registrations/verifications", new VerifyEventRegistrationRequest(firstToken));
        Assert.Equal(HttpStatusCode.BadRequest, oldTokenResponse.StatusCode);

        var newTokenResponse = await _client.PostAsJsonAsync(
            "/api/v1/public/event-registrations/verifications", new VerifyEventRegistrationRequest(secondToken));
        Assert.Equal(HttpStatusCode.NoContent, newTokenResponse.StatusCode);
    }

    // --- Cancellation ---

    [Fact]
    public async Task CancelEventRegistration_ViaLink_IsIdempotent_AndReleasesConfirmedSlot()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(accessToken, eventTypeId, NewScheduleRequest());

        var (email, userAccessToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, email, privacyVersion, userAccessToken);
        var decisionEmail = Assert.Single(_factory.EmailSender.SentEmails, e => e.ToEmail == email);
        var cancelToken = ExtractQueryParam(decisionEmail.Body, "token");

        var firstCancelResponse = await _client.PostAsJsonAsync(
            "/api/v1/public/event-registrations/cancellations", new CancelEventRegistrationRequest(cancelToken));
        Assert.Equal(HttpStatusCode.NoContent, firstCancelResponse.StatusCode);

        var (confirmedCount, _) = await _factory.GetEventScheduleCountersAsync(contentItemId);
        Assert.Equal(0, confirmedCount);
        var snapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, email);
        Assert.Equal(EventRegistrationStatus.Cancelled, snapshot!.Status);

        var secondCancelResponse = await _client.PostAsJsonAsync(
            "/api/v1/public/event-registrations/cancellations", new CancelEventRegistrationRequest(cancelToken));
        Assert.Equal(HttpStatusCode.NoContent, secondCancelResponse.StatusCode);
        var (confirmedCountAfterSecondCall, _) = await _factory.GetEventScheduleCountersAsync(contentItemId);
        Assert.Equal(0, confirmedCountAfterSecondCall);
    }

    [Fact]
    public async Task CancelEventRegistration_WithUnknownToken_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/public/event-registrations/cancellations", new CancelEventRegistrationRequest("unknown-token"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CancelEventRegistration_AfterEventHasStarted_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(startsAtUtc: DateTime.UtcNow.AddSeconds(4)));

        var (email, userAccessToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, email, privacyVersion, userAccessToken);
        var decisionEmail = Assert.Single(_factory.EmailSender.SentEmails, e => e.ToEmail == email);
        var cancelToken = ExtractQueryParam(decisionEmail.Body, "token");

        await Task.Delay(TimeSpan.FromSeconds(5));

        var response = await _client.PostAsJsonAsync(
            "/api/v1/public/event-registrations/cancellations", new CancelEventRegistrationRequest(cancelToken));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // --- Concurrency: ADR-024 §11.2 "kapasite 5, 20 eşzamanlı doğrulama" ---

    [Fact]
    public async Task VerifyEventRegistration_TwentyConcurrentVerifications_WithCapacityFive_ConfirmsExactlyFive()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(capacity: 5, autoConfirm: true, waitlistEnabled: true));

        const int registrantCount = 20;
        var emails = Enumerable.Range(0, registrantCount).Select(i => $"race-{i}-{Guid.NewGuid():N}@example.com").ToList();

        var submissionTokens = new List<string>();
        foreach (var _ in emails)
        {
            var response = await _client.GetAsync("/api/v1/public/submission-tokens");
            var token = await response.Content.ReadFromJsonAsync<GetSubmissionTokenResponse>();
            submissionTokens.Add(token!.Token);
        }

        await Task.Delay(TimeSpan.FromSeconds(3.2));

        for (var i = 0; i < registrantCount; i++)
        {
            var response = await CreateAnonymousRegistrationAsync(contentItemId, emails[i], privacyVersion, submissionTokens[i]);
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }

        var verificationTokens = emails
            .Select(email => ExtractQueryParam(Assert.Single(_factory.EmailSender.SentEmails, e => e.ToEmail == email).Body, "token"))
            .ToList();

        var verifyTasks = verificationTokens
            .Select(verificationToken => _client.PostAsJsonAsync(
                "/api/v1/public/event-registrations/verifications", new VerifyEventRegistrationRequest(verificationToken)))
            .ToArray();
        var verifyResponses = await Task.WhenAll(verifyTasks);

        Assert.All(verifyResponses, r => Assert.Equal(HttpStatusCode.NoContent, r.StatusCode));

        var (confirmedCount, waitlistedCount) = await _factory.GetEventScheduleCountersAsync(contentItemId);
        Assert.Equal(5, confirmedCount);
        Assert.Equal(15, waitlistedCount);

        var statuses = await _factory.GetEventRegistrationStatusesAsync(contentItemId);
        Assert.Equal(5, statuses.Count(s => s == EventRegistrationStatus.Confirmed));
        Assert.Equal(15, statuses.Count(s => s == EventRegistrationStatus.Waitlisted));
    }
}
