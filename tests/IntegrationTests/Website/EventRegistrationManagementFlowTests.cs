using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.AdminCancelEventRegistration;
using GenclikMerkezi.Modules.Website.Features.CancelEventSchedule;
using GenclikMerkezi.Modules.Website.Features.ConfirmEventRegistration;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.CreateEventRegistration;
using GenclikMerkezi.Modules.Website.Features.CreateLegalDocument;
using GenclikMerkezi.Modules.Website.Features.CreateLegalDocumentDraft;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.GetEventRegistrationById;
using GenclikMerkezi.Modules.Website.Features.GetEventRegistrations;
using GenclikMerkezi.Modules.Website.Features.GetLegalDocumentById;
using GenclikMerkezi.Modules.Website.Features.GetSiteSettings;
using GenclikMerkezi.Modules.Website.Features.MarkEventRegistrationAttended;
using GenclikMerkezi.Modules.Website.Features.MarkEventRegistrationNoShow;
using GenclikMerkezi.Modules.Website.Features.PublishContentItem;
using GenclikMerkezi.Modules.Website.Features.PublishLegalDocumentDraft;
using GenclikMerkezi.Modules.Website.Features.RejectEventRegistration;
using GenclikMerkezi.Modules.Website.Features.UpdateLegalDocumentDraftBody;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsEvents;
using GenclikMerkezi.Modules.Website.Features.UpsertEventSchedule;
using GenclikMerkezi.Modules.Website.Features.WaitlistEventRegistration;
using GenclikMerkezi.Modules.Website.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §11.2 (Faz 4 Görev 4). Shares the "one CustomWebApplicationFactory/one Sqlite database per
// class" caveat every other Website flow test class documents. Drives registrations into each starting
// status via the authenticated public registration path (RegisterVerifiedUserAsync +
// CreateAuthenticatedRegistrationAsync), which decides the capacity outcome synchronously - no
// verification-email wait needed, mirroring EventRegistrationFlowTests' own setup.
public class EventRegistrationManagementFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly CreateContentItemSeoInput EmptySeo = new(null, null, null, null, null, null, null, false);

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EventRegistrationManagementFlowTests(CustomWebApplicationFactory factory)
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
        byte[]? rowVersion = null,
        DateTime? startsAtUtc = null,
        int? capacity = 10,
        bool autoConfirm = true,
        bool waitlistEnabled = true) =>
        new(
            rowVersion, startsAtUtc ?? DateTime.UtcNow.AddDays(10), (startsAtUtc ?? DateTime.UtcNow.AddDays(10)).AddHours(2), "InPerson", null,
            capacity, true, null, null, null, null, autoConfirm, waitlistEnabled,
            [new UpsertEventScheduleTranslationInput("tr", "Gençlik Merkezi Salonu", "Örnek Adres No:1", "Ücretsiz", "Eğitmen", "<p>Program</p>", "Not")]);

    private async Task<EventScheduleDetailResponseShim> UpsertScheduleAsync(string accessToken, Guid contentItemId, UpsertEventScheduleRequest request)
    {
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken, request));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<EventScheduleDetailResponseShim>())!;
    }

    private async Task<Guid> CreatePublishedEventAsync(string accessToken, Guid contentTypeId, UpsertEventScheduleRequest scheduleRequest)
    {
        var id = await CreateContentItemAsync(accessToken, contentTypeId, $"Etkinlik-{Guid.NewGuid():N}");
        await PublishAsync(accessToken, id);
        await UpsertScheduleAsync(accessToken, id, scheduleRequest);
        return id;
    }

    private async Task<HttpResponseMessage> CreateAuthenticatedRegistrationAsync(
        Guid contentItemId, string email, int privacyVersion, string accessToken) =>
        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/public/events/{contentItemId}/registrations", accessToken,
            new CreateEventRegistrationRequest("", "", "", "Ahmet", "Yılmaz", email, null, "tr", privacyVersion)));

    private async Task<EventRegistrationDetailResponse> GetRegistrationAsync(string accessToken, Guid contentItemId, Guid registrationId)
    {
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{contentItemId}/event/registrations/{registrationId}", accessToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<EventRegistrationDetailResponse>())!;
    }

    private Task<HttpResponseMessage> ConfirmAsync(string accessToken, Guid contentItemId, Guid registrationId, byte[] rowVersion) =>
        _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{contentItemId}/event/registrations/{registrationId}/confirm", accessToken,
            new ConfirmEventRegistrationRequest(rowVersion)));

    private Task<HttpResponseMessage> WaitlistAsync(string accessToken, Guid contentItemId, Guid registrationId, byte[] rowVersion) =>
        _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{contentItemId}/event/registrations/{registrationId}/waitlist", accessToken,
            new WaitlistEventRegistrationRequest(rowVersion)));

    private Task<HttpResponseMessage> RejectAsync(string accessToken, Guid contentItemId, Guid registrationId, byte[] rowVersion) =>
        _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{contentItemId}/event/registrations/{registrationId}/reject", accessToken,
            new RejectEventRegistrationRequest(rowVersion, null)));

    private Task<HttpResponseMessage> AdminCancelAsync(string accessToken, Guid contentItemId, Guid registrationId, byte[] rowVersion) =>
        _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{contentItemId}/event/registrations/{registrationId}/cancel", accessToken,
            new AdminCancelEventRegistrationRequest(rowVersion)));

    private Task<HttpResponseMessage> MarkAttendedAsync(string accessToken, Guid contentItemId, Guid registrationId, byte[] rowVersion) =>
        _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{contentItemId}/event/registrations/{registrationId}/attended", accessToken,
            new MarkEventRegistrationAttendedRequest(rowVersion)));

    private Task<HttpResponseMessage> MarkNoShowAsync(string accessToken, Guid contentItemId, Guid registrationId, byte[] rowVersion) =>
        _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{contentItemId}/event/registrations/{registrationId}/no-show", accessToken,
            new MarkEventRegistrationNoShowRequest(rowVersion)));

    private async Task<int> GetPersonalDataAccessLogCountAsync(PersonalDataEntityType entityType, Guid? entityId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WebsiteDbContext>();
        return await dbContext.PersonalDataAccessLogs.CountAsync(
            l => l.EntityType == entityType && l.EntityId == entityId && l.Action == PersonalDataAccessAction.View);
    }

    [Fact]
    public async Task Confirm_FromApplied_Succeeds_AndIncrementsConfirmedCount()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(capacity: 10, autoConfirm: false, waitlistEnabled: true));

        var (email, userAccessToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, email, privacyVersion, userAccessToken);
        var snapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, email);
        Assert.Equal(EventRegistrationStatus.Applied, snapshot!.Status);
        var detail = await GetRegistrationAsync(accessToken, contentItemId, snapshot.Id);

        var response = await ConfirmAsync(accessToken, contentItemId, snapshot.Id, detail.RowVersion);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var (confirmedCount, _) = await _factory.GetEventScheduleCountersAsync(contentItemId);
        Assert.Equal(1, confirmedCount);
    }

    [Fact]
    public async Task Confirm_FromWaitlisted_PromotesAndUpdatesBothCounters()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(capacity: 1, autoConfirm: true, waitlistEnabled: true));

        var (firstEmail, firstToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, firstEmail, privacyVersion, firstToken);

        var (secondEmail, secondToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, secondEmail, privacyVersion, secondToken);
        var waitlistedSnapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, secondEmail);
        Assert.Equal(EventRegistrationStatus.Waitlisted, waitlistedSnapshot!.Status);
        var detail = await GetRegistrationAsync(accessToken, contentItemId, waitlistedSnapshot.Id);

        // Open up a confirmed slot (cancel the first) so the promotion below has room.
        var firstSnapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, firstEmail);
        var firstDetail = await GetRegistrationAsync(accessToken, contentItemId, firstSnapshot!.Id);
        await AdminCancelAsync(accessToken, contentItemId, firstSnapshot.Id, firstDetail.RowVersion);

        var response = await ConfirmAsync(accessToken, contentItemId, waitlistedSnapshot.Id, detail.RowVersion);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var (confirmedCount, waitlistedCount) = await _factory.GetEventScheduleCountersAsync(contentItemId);
        Assert.Equal(1, confirmedCount);
        Assert.Equal(0, waitlistedCount);
        var promoted = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, secondEmail);
        Assert.Equal(EventRegistrationStatus.Confirmed, promoted!.Status);
    }

    [Fact]
    public async Task Confirm_WhenCapacityFull_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(capacity: 1, autoConfirm: false, waitlistEnabled: true));

        var (firstEmail, firstToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, firstEmail, privacyVersion, firstToken);
        var firstSnapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, firstEmail);
        var firstDetail = await GetRegistrationAsync(accessToken, contentItemId, firstSnapshot!.Id);
        await ConfirmAsync(accessToken, contentItemId, firstSnapshot.Id, firstDetail.RowVersion);

        var (secondEmail, secondToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, secondEmail, privacyVersion, secondToken);
        var secondSnapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, secondEmail);
        var secondDetail = await GetRegistrationAsync(accessToken, contentItemId, secondSnapshot!.Id);

        var response = await ConfirmAsync(accessToken, contentItemId, secondSnapshot.Id, secondDetail.RowVersion);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Waitlist_FromApplied_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(capacity: 10, autoConfirm: false, waitlistEnabled: true));

        var (email, userAccessToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, email, privacyVersion, userAccessToken);
        var snapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, email);
        var detail = await GetRegistrationAsync(accessToken, contentItemId, snapshot!.Id);

        var response = await WaitlistAsync(accessToken, contentItemId, snapshot.Id, detail.RowVersion);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var updated = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, email);
        Assert.Equal(EventRegistrationStatus.Waitlisted, updated!.Status);
        var (_, waitlistedCount) = await _factory.GetEventScheduleCountersAsync(contentItemId);
        Assert.Equal(1, waitlistedCount);
    }

    [Fact]
    public async Task Reject_FromWaitlisted_ReleasesWaitlistSlot()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(capacity: 1, autoConfirm: true, waitlistEnabled: true));

        var (firstEmail, firstToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, firstEmail, privacyVersion, firstToken);

        var (secondEmail, secondToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, secondEmail, privacyVersion, secondToken);
        var waitlistedSnapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, secondEmail);
        var detail = await GetRegistrationAsync(accessToken, contentItemId, waitlistedSnapshot!.Id);

        var response = await RejectAsync(accessToken, contentItemId, waitlistedSnapshot.Id, detail.RowVersion);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var (_, waitlistedCount) = await _factory.GetEventScheduleCountersAsync(contentItemId);
        Assert.Equal(0, waitlistedCount);
        var rejected = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, secondEmail);
        Assert.Equal(EventRegistrationStatus.Rejected, rejected!.Status);
    }

    [Fact]
    public async Task AdminCancel_FromConfirmed_ReleasesConfirmedSlot_AndSendsNotice()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(capacity: 10, autoConfirm: true, waitlistEnabled: true));

        var (email, userAccessToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, email, privacyVersion, userAccessToken);
        var snapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, email);
        var detail = await GetRegistrationAsync(accessToken, contentItemId, snapshot!.Id);

        var response = await AdminCancelAsync(accessToken, contentItemId, snapshot.Id, detail.RowVersion);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var (confirmedCount, _) = await _factory.GetEventScheduleCountersAsync(contentItemId);
        Assert.Equal(0, confirmedCount);
        var cancelled = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, email);
        Assert.Equal(EventRegistrationStatus.Cancelled, cancelled!.Status);
        Assert.Contains(_factory.EmailSender.SentEmails, e => e.ToEmail == email);
    }

    [Fact]
    public async Task MarkAttended_BeforeEventStarts_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(capacity: 10, autoConfirm: true, waitlistEnabled: true));

        var (email, userAccessToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, email, privacyVersion, userAccessToken);
        var snapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, email);
        var detail = await GetRegistrationAsync(accessToken, contentItemId, snapshot!.Id);

        var response = await MarkAttendedAsync(accessToken, contentItemId, snapshot.Id, detail.RowVersion);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task MarkAttended_AfterEventStarts_FromConfirmed_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(capacity: 10, autoConfirm: true, waitlistEnabled: true));

        var (email, userAccessToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, email, privacyVersion, userAccessToken);
        var snapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, email);

        // The event hasn't started yet (old StartsAtUtc is still in the future), so moving it into the
        // past is a legal update - UpdateSchedule only blocks changes once `now >= StartsAtUtc`.
        var scheduleBefore = await _factory.GetEventScheduleCountersAsync(contentItemId);
        _ = scheduleBefore;
        var scheduleDetailResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken));
        var scheduleDetail = await scheduleDetailResponse.Content.ReadFromJsonAsync<EventScheduleDetailResponseShim>();
        await UpsertScheduleAsync(
            accessToken, contentItemId,
            NewScheduleRequest(scheduleDetail!.RowVersion, startsAtUtc: DateTime.UtcNow.AddHours(-2), capacity: 10));

        var detail = await GetRegistrationAsync(accessToken, contentItemId, snapshot!.Id);
        var response = await MarkAttendedAsync(accessToken, contentItemId, snapshot.Id, detail.RowVersion);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var updated = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, email);
        Assert.Equal(EventRegistrationStatus.Attended, updated!.Status);
        var (confirmedCount, _) = await _factory.GetEventScheduleCountersAsync(contentItemId);
        Assert.Equal(1, confirmedCount);
    }

    [Fact]
    public async Task MarkNoShow_AfterEventStarts_FromConfirmed_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(capacity: 10, autoConfirm: true, waitlistEnabled: true));

        var (email, userAccessToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, email, privacyVersion, userAccessToken);
        var snapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, email);

        var scheduleDetailResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken));
        var scheduleDetail = await scheduleDetailResponse.Content.ReadFromJsonAsync<EventScheduleDetailResponseShim>();
        await UpsertScheduleAsync(
            accessToken, contentItemId,
            NewScheduleRequest(scheduleDetail!.RowVersion, startsAtUtc: DateTime.UtcNow.AddHours(-2), capacity: 10));

        var detail = await GetRegistrationAsync(accessToken, contentItemId, snapshot!.Id);
        var response = await MarkNoShowAsync(accessToken, contentItemId, snapshot.Id, detail.RowVersion);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var updated = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, email);
        Assert.Equal(EventRegistrationStatus.NoShow, updated!.Status);
        var (confirmedCount, _) = await _factory.GetEventScheduleCountersAsync(contentItemId);
        Assert.Equal(1, confirmedCount);
    }

    [Fact]
    public async Task GetEventRegistrationById_Succeeds_WritesPersonalDataAccessLogEntry()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(capacity: 10, autoConfirm: false, waitlistEnabled: true));

        var (email, userAccessToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, email, privacyVersion, userAccessToken);
        var snapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, email);

        var countBefore = await GetPersonalDataAccessLogCountAsync(PersonalDataEntityType.EventRegistration, snapshot!.Id);
        await GetRegistrationAsync(accessToken, contentItemId, snapshot.Id);
        var countAfter = await GetPersonalDataAccessLogCountAsync(PersonalDataEntityType.EventRegistration, snapshot.Id);

        Assert.Equal(countBefore + 1, countAfter);
    }

    [Fact]
    public async Task GetEventRegistrations_List_Succeeds_WritesPersonalDataAccessLogEntry_AndReturnsCounters()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(capacity: 10, autoConfirm: true, waitlistEnabled: true));

        var (email, userAccessToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, email, privacyVersion, userAccessToken);

        var countBefore = await GetPersonalDataAccessLogCountAsync(PersonalDataEntityType.EventRegistration, null);
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{contentItemId}/event/registrations", accessToken));
        var countAfter = await GetPersonalDataAccessLogCountAsync(PersonalDataEntityType.EventRegistration, null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<GetEventRegistrationsResponse>();
        Assert.Equal(1, list!.Counters.Confirmed);
        Assert.Single(list.Registrations.Items);
        Assert.Equal(countBefore + 1, countAfter);
    }

    [Fact]
    public async Task AdminEndpoints_WithoutAuthentication_ReturnUnauthorized()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(capacity: 10, autoConfirm: true, waitlistEnabled: true));
        _ = privacyVersion;

        var listResponse = await _client.GetAsync($"/api/v1/admin/website/contents/{contentItemId}/event/registrations");
        var confirmResponse = await _client.PostAsJsonAsync(
            $"/api/v1/admin/website/contents/{contentItemId}/event/registrations/{Guid.NewGuid()}/confirm",
            new ConfirmEventRegistrationRequest([1, 2, 3]));

        Assert.Equal(HttpStatusCode.Unauthorized, listResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, confirmResponse.StatusCode);
    }

    [Fact]
    public async Task Confirm_TwoConcurrentRequests_WithCapacityOne_ExactlyOneSucceeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(capacity: 1, autoConfirm: false, waitlistEnabled: false));

        var (firstEmail, firstToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, firstEmail, privacyVersion, firstToken);
        var firstSnapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, firstEmail);
        var firstDetail = await GetRegistrationAsync(accessToken, contentItemId, firstSnapshot!.Id);

        var (secondEmail, secondToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, secondEmail, privacyVersion, secondToken);
        var secondSnapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, secondEmail);
        var secondDetail = await GetRegistrationAsync(accessToken, contentItemId, secondSnapshot!.Id);

        var responses = await Task.WhenAll(
            ConfirmAsync(accessToken, contentItemId, firstSnapshot.Id, firstDetail.RowVersion),
            ConfirmAsync(accessToken, contentItemId, secondSnapshot.Id, secondDetail.RowVersion));

        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.NoContent);
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Conflict);
        var (confirmedCount, _) = await _factory.GetEventScheduleCountersAsync(contentItemId);
        Assert.Equal(1, confirmedCount);
    }

    [Fact]
    public async Task CancelEventSchedule_NotifiesOnlyApplicableRegistrations_WithoutChangingTheirStatus()
    {
        var accessToken = await LoginAsAdminAsync();
        var (_, privacyVersion) = await EnableEventRegistrationAsync(accessToken);
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(capacity: 10, autoConfirm: false, waitlistEnabled: true));

        var (appliedEmail, appliedToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, appliedEmail, privacyVersion, appliedToken);
        var appliedSnapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, appliedEmail);
        var appliedDetail = await GetRegistrationAsync(accessToken, contentItemId, appliedSnapshot!.Id);

        var (rejectedEmail, rejectedToken, _) = await RegisterVerifiedUserAsync();
        await CreateAuthenticatedRegistrationAsync(contentItemId, rejectedEmail, privacyVersion, rejectedToken);
        var rejectedSnapshot = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, rejectedEmail);
        var rejectedDetail = await GetRegistrationAsync(accessToken, contentItemId, rejectedSnapshot!.Id);
        await RejectAsync(accessToken, contentItemId, rejectedSnapshot.Id, rejectedDetail.RowVersion);

        var scheduleResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken));
        var scheduleDetail = await scheduleResponse.Content.ReadFromJsonAsync<EventScheduleDetailResponseShim>();

        var cancelResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{contentItemId}/event/cancel", accessToken,
            new CancelEventScheduleRequest(scheduleDetail!.RowVersion, "Hava koşulları nedeniyle iptal edildi.")));
        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

        // Filtered to the cancellation notice specifically (by subject prefix) - both addresses also
        // received unrelated emails earlier in this test (the "Başvurunuz alındı" applied notice at
        // creation, and the "Başvurunuz kabul edilmedi" rejection notice for rejectedEmail), which a
        // plain "was this address ever emailed" check would wrongly conflate with the fan-out itself.
        Assert.Contains(_factory.EmailSender.SentEmails, e => e.ToEmail == appliedEmail && e.Subject.StartsWith("Etkinlik iptal edildi"));
        Assert.DoesNotContain(_factory.EmailSender.SentEmails, e => e.ToEmail == rejectedEmail && e.Subject.StartsWith("Etkinlik iptal edildi"));

        var appliedAfter = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, appliedEmail);
        var rejectedAfter = await _factory.GetEventRegistrationSnapshotAsync(contentItemId, rejectedEmail);
        Assert.Equal(EventRegistrationStatus.Applied, appliedAfter!.Status);
        Assert.Equal(EventRegistrationStatus.Rejected, rejectedAfter!.Status);
    }

    // Local shim: only the RowVersion field this test class needs from
    // GetEventScheduleByContentItemId/UpsertEventSchedule's actual response, deserialized by property
    // name (System.Text.Json ignores extra response fields it doesn't declare).
    private sealed record EventScheduleDetailResponseShim(byte[] RowVersion);
}
