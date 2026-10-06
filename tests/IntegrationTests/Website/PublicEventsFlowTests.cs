using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.GetEventScheduleByContentItemId;
using GenclikMerkezi.Modules.Website.Features.GetPublicContentById;
using GenclikMerkezi.Modules.Website.Features.GetPublicContents;
using GenclikMerkezi.Modules.Website.Features.GetPublicEvents;
using GenclikMerkezi.Modules.Website.Features.PublishContentItem;
using GenclikMerkezi.Modules.Website.Features.UpsertEventSchedule;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §17/§11 (Faz 4 Görev 2). Shares the "one CustomWebApplicationFactory/one Sqlite database per
// class" caveat every other Website flow test class documents. Uses the seeded "event" and "training"
// ContentTypes (both SupportsEvent + EventDateAsc) and "news" (not SupportsEvent) from
// GenclikMerkeziContentTypeSeed.
public class PublicEventsFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly CreateContentItemSeoInput EmptySeo = new(null, null, null, null, null, null, null, false);

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PublicEventsFlowTests(CustomWebApplicationFactory factory)
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
        var detail = await detailResponse.Content.ReadFromJsonAsync<GenclikMerkezi.Modules.Website.Features.GetContentItemById.ContentItemDetailResponse>();
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{contentItemId}/publish", accessToken,
            new PublishContentItemRequest(detail!.RowVersion, null, null)));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private static UpsertEventScheduleRequest NewScheduleRequest(
        DateTime? startsAtUtc = null,
        DateTime? endsAtUtc = null,
        string format = "InPerson",
        string? onlineLink = null,
        int? capacity = null,
        bool registrationEnabled = true,
        DateTime? registrationOpensAtUtc = null,
        DateTime? registrationClosesAtUtc = null,
        bool autoConfirm = true,
        bool waitlistEnabled = true,
        string venueName = "Gençlik Merkezi Salonu") =>
        new(
            null, startsAtUtc ?? DateTime.UtcNow.AddDays(10), endsAtUtc ?? (startsAtUtc ?? DateTime.UtcNow.AddDays(10)).AddHours(2), format,
            onlineLink, capacity, registrationEnabled, registrationOpensAtUtc, registrationClosesAtUtc, null, null, autoConfirm,
            waitlistEnabled,
            [new UpsertEventScheduleTranslationInput("tr", venueName, "Örnek Adres No:1", "Ücretsiz", "Eğitmen", "<p>Program</p>", "Not")]);

    private async Task UpsertScheduleAsync(string accessToken, Guid contentItemId, UpsertEventScheduleRequest request)
    {
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken, request));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<(Guid Id, string Title)> CreatePublishedEventAsync(
        string accessToken, Guid contentTypeId, UpsertEventScheduleRequest scheduleRequest)
    {
        var title = $"Etkinlik-{Guid.NewGuid():N}";
        var id = await CreateContentItemAsync(accessToken, contentTypeId, title);
        await PublishAsync(accessToken, id);
        await UpsertScheduleAsync(accessToken, id, scheduleRequest);
        return (id, title);
    }

    private async Task<HttpResponseMessage> GetEventsAsync(string queryString = "") =>
        await _client.GetAsync($"/api/v1/public/events{queryString}");

    // --- GetPublicEvents: membership and sorting ---

    [Fact]
    public async Task GetPublicEvents_ItemWithoutSchedule_IsExcluded()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var id = await CreateContentItemAsync(accessToken, eventTypeId, $"NoSchedule-{Guid.NewGuid():N}");
        await PublishAsync(accessToken, id);

        var response = await GetEventsAsync("?when=all&pageSize=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PagedResult<PublicEventListItemResponse>>();
        Assert.DoesNotContain(body!.Items, i => i.Id == id);
    }

    [Fact]
    public async Task GetPublicEvents_UpcomingWindow_SortsByStartsAtUtcAscending()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var sooner = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(startsAtUtc: DateTime.UtcNow.AddDays(1)));
        var later = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(startsAtUtc: DateTime.UtcNow.AddDays(5)));

        var response = await GetEventsAsync("?pageSize=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PagedResult<PublicEventListItemResponse>>();
        var items = body!.Items.ToList();
        var sonerIndex = items.FindIndex(i => i.Id == sooner.Id);
        var laterIndex = items.FindIndex(i => i.Id == later.Id);
        Assert.True(sonerIndex >= 0 && laterIndex >= 0 && sonerIndex < laterIndex);
    }

    [Fact]
    public async Task GetPublicEvents_PastWindow_SortsDescendingAndExcludesUpcoming()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var past = await CreatePublishedEventAsync(
            accessToken, eventTypeId,
            NewScheduleRequest(startsAtUtc: DateTime.UtcNow.AddDays(-5), endsAtUtc: DateTime.UtcNow.AddDays(-5).AddHours(2)));
        var upcoming = await CreatePublishedEventAsync(accessToken, eventTypeId, NewScheduleRequest(startsAtUtc: DateTime.UtcNow.AddDays(5)));

        var response = await GetEventsAsync("?when=past&pageSize=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PagedResult<PublicEventListItemResponse>>();
        Assert.Contains(body!.Items, i => i.Id == past.Id);
        Assert.DoesNotContain(body.Items, i => i.Id == upcoming.Id);
    }

    [Fact]
    public async Task GetPublicEvents_TypeKeyFilter_NarrowsToThatType()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var trainingTypeId = await GetContentTypeIdAsync(accessToken, "training");
        var eventItem = await CreatePublishedEventAsync(accessToken, eventTypeId, NewScheduleRequest());
        var trainingItem = await CreatePublishedEventAsync(accessToken, trainingTypeId, NewScheduleRequest());

        var response = await GetEventsAsync("?typeKey=training&pageSize=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PagedResult<PublicEventListItemResponse>>();
        Assert.Contains(body!.Items, i => i.Id == trainingItem.Id);
        Assert.DoesNotContain(body.Items, i => i.Id == eventItem.Id);
    }

    [Fact]
    public async Task GetPublicEvents_UnknownTypeKey_ReturnsNotFound()
    {
        var response = await GetEventsAsync($"?typeKey=bilinmeyen-{Guid.NewGuid():N}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicEvents_TypeKeyNotSupportingEvents_ReturnsBadRequest()
    {
        var response = await GetEventsAsync("?typeKey=news");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicEvents_InvalidWhen_ReturnsBadRequest()
    {
        var response = await GetEventsAsync("?when=yesterday");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicEvents_InvalidFormat_ReturnsBadRequest()
    {
        var response = await GetEventsAsync("?format=Teleport");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicEvents_FormatFilter_NarrowsToThatFormat()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var inPerson = await CreatePublishedEventAsync(accessToken, eventTypeId, NewScheduleRequest(format: "InPerson"));
        var online = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(format: "Online", onlineLink: "https://meet.example.com/abc"));

        var response = await GetEventsAsync("?format=Online&pageSize=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PagedResult<PublicEventListItemResponse>>();
        Assert.Contains(body!.Items, i => i.Id == online.Id);
        Assert.DoesNotContain(body.Items, i => i.Id == inPerson.Id);
    }

    // --- GetPublicEvents: registrationState / remainingSpots ---

    [Fact]
    public async Task GetPublicEvents_OpenRegistrationNoCapacity_ReturnsOpenWithNullRemainingSpots()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var item = await CreatePublishedEventAsync(accessToken, eventTypeId, NewScheduleRequest(capacity: null));

        var response = await GetEventsAsync("?pageSize=100");
        var body = await response.Content.ReadFromJsonAsync<PagedResult<PublicEventListItemResponse>>();
        var listed = Assert.Single(body!.Items, i => i.Id == item.Id);

        Assert.Equal("Open", listed.Event.RegistrationState);
        Assert.Null(listed.Event.RemainingSpots);
    }

    [Fact]
    public async Task GetPublicEvents_CapacitySet_ReturnsRemainingSpotsEqualToCapacity()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var item = await CreatePublishedEventAsync(accessToken, eventTypeId, NewScheduleRequest(capacity: 5));

        var response = await GetEventsAsync("?pageSize=100");
        var body = await response.Content.ReadFromJsonAsync<PagedResult<PublicEventListItemResponse>>();
        var listed = Assert.Single(body!.Items, i => i.Id == item.Id);

        Assert.Equal(5, listed.Event.RemainingSpots);
    }

    [Fact]
    public async Task GetPublicEvents_RegistrationNotYetOpen_ReturnsNotOpen()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var item = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(registrationOpensAtUtc: DateTime.UtcNow.AddDays(1)));

        var response = await GetEventsAsync("?pageSize=100");
        var body = await response.Content.ReadFromJsonAsync<PagedResult<PublicEventListItemResponse>>();
        var listed = Assert.Single(body!.Items, i => i.Id == item.Id);

        Assert.Equal("NotOpen", listed.Event.RegistrationState);
    }

    [Fact]
    public async Task GetPublicEvents_RegistrationDisabled_ReturnsClosed()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var item = await CreatePublishedEventAsync(accessToken, eventTypeId, NewScheduleRequest(registrationEnabled: false));

        var response = await GetEventsAsync("?pageSize=100");
        var body = await response.Content.ReadFromJsonAsync<PagedResult<PublicEventListItemResponse>>();
        var listed = Assert.Single(body!.Items, i => i.Id == item.Id);

        Assert.Equal("Closed", listed.Event.RegistrationState);
    }

    [Fact]
    public async Task GetPublicEvents_CancelledEvent_ReturnsCancelled()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var item = await CreatePublishedEventAsync(accessToken, eventTypeId, NewScheduleRequest());

        var scheduleResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{item.Id}/event", accessToken));
        var schedule = await scheduleResponse.Content.ReadFromJsonAsync<EventScheduleDetailResponse>();
        var cancelResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{item.Id}/event/cancel", accessToken,
            new GenclikMerkezi.Modules.Website.Features.CancelEventSchedule.CancelEventScheduleRequest(schedule!.RowVersion, "Test")));
        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

        var response = await GetEventsAsync("?when=all&pageSize=100");
        var body = await response.Content.ReadFromJsonAsync<PagedResult<PublicEventListItemResponse>>();
        var listed = Assert.Single(body!.Items, i => i.Id == item.Id);

        Assert.Equal("Cancelled", listed.Event.RegistrationState);
        Assert.True(listed.Event.IsCancelled);
    }

    // --- GetPublicEvents: never leaks OnlineLink ---

    [Fact]
    public async Task GetPublicEvents_ResponseBody_NeverContainsOnlineLink()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        const string onlineLink = "https://meet.example.com/very-secret-room";
        await CreatePublishedEventAsync(accessToken, eventTypeId, NewScheduleRequest(format: "Online", onlineLink: onlineLink));

        var response = await GetEventsAsync("?pageSize=100");
        var raw = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain(onlineLink, raw);
    }

    // --- GetPublicContents: EventDateAsc sorting (Görev 2 bullet 1) ---

    [Fact]
    public async Task GetPublicContents_EventType_SortsByStartsAtUtcAscendingWithUnscheduledLast()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var later = await CreatePublishedEventAsync(accessToken, eventTypeId, NewScheduleRequest(startsAtUtc: DateTime.UtcNow.AddDays(10)));
        var sooner = await CreatePublishedEventAsync(accessToken, eventTypeId, NewScheduleRequest(startsAtUtc: DateTime.UtcNow.AddDays(2)));
        var noScheduleId = await CreateContentItemAsync(accessToken, eventTypeId, $"NoSchedule-{Guid.NewGuid():N}");
        await PublishAsync(accessToken, noScheduleId);

        var response = await _client.GetAsync("/api/v1/public/contents?type=event&pageSize=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PublicContentListResponse>();
        var items = body!.Items.Items.ToList();
        var soonerIndex = items.FindIndex(i => i.Id == sooner.Id);
        var laterIndex = items.FindIndex(i => i.Id == later.Id);
        var noScheduleIndex = items.FindIndex(i => i.Id == noScheduleId);

        Assert.True(soonerIndex >= 0 && laterIndex >= 0 && noScheduleIndex >= 0);
        Assert.True(soonerIndex < laterIndex);
        Assert.True(laterIndex < noScheduleIndex);
    }

    // --- GetPublicContentById: event field ---

    [Fact]
    public async Task GetPublicContentById_EventType_IncludesEventFieldWithoutOnlineLink()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        const string onlineLink = "https://meet.example.com/detail-secret";
        var item = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(format: "Hybrid", onlineLink: onlineLink, capacity: 3));

        var response = await _client.GetAsync($"/api/v1/public/contents/{item.Id}");
        var raw = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain(onlineLink, raw);
        var detail = await response.Content.ReadFromJsonAsync<PublicContentDetailResponse>();
        Assert.NotNull(detail!.Event);
        Assert.Equal("Hybrid", detail.Event!.Format);
        Assert.Equal(3, detail.Event.RemainingSpots);
        Assert.Equal("Open", detail.Event.RegistrationState);
        Assert.Equal("Gençlik Merkezi Salonu", detail.Event.VenueName);
    }

    // --- GetPublicEventCalendar ---

    [Fact]
    public async Task GetPublicEventCalendar_UnknownContentItem_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/v1/public/events/{Guid.NewGuid()}/calendar.ics");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicEventCalendar_DraftContentItem_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var id = await CreateContentItemAsync(accessToken, eventTypeId, $"Draft-{Guid.NewGuid():N}");

        var response = await _client.GetAsync($"/api/v1/public/events/{id}/calendar.ics");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicEventCalendar_PublishedEventWithSchedule_ReturnsExpectedIcsFields()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var startsAtUtc = new DateTime(2026, 11, 20, 18, 0, 0, DateTimeKind.Utc);
        const string onlineLink = "https://meet.example.com/ics-secret";
        var item = await CreatePublishedEventAsync(
            accessToken, eventTypeId,
            NewScheduleRequest(startsAtUtc: startsAtUtc, endsAtUtc: startsAtUtc.AddHours(2), format: "Hybrid", onlineLink: onlineLink));

        var response = await _client.GetAsync($"/api/v1/public/events/{item.Id}/calendar.ics");
        var ics = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("text/calendar", response.Content.Headers.ContentType!.MediaType);
        Assert.Contains("BEGIN:VEVENT", ics);
        Assert.Contains($"UID:{item.Id}@", ics);
        Assert.Contains("DTSTART:20261120T180000Z", ics);
        Assert.Contains("DTEND:20261120T200000Z", ics);
        Assert.Contains("LOCATION:Gençlik Merkezi Salonu", ics);
        Assert.DoesNotContain(onlineLink, ics);
        Assert.DoesNotContain("STATUS:CANCELLED", ics);
    }

    [Fact]
    public async Task GetPublicEventCalendar_OnlineOnlyFormat_LocationIsOnline()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var item = await CreatePublishedEventAsync(
            accessToken, eventTypeId, NewScheduleRequest(format: "Online", onlineLink: "https://meet.example.com/x"));

        var response = await _client.GetAsync($"/api/v1/public/events/{item.Id}/calendar.ics");
        var ics = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("LOCATION:Online", ics);
    }

    [Fact]
    public async Task GetPublicEventCalendar_CancelledEvent_IncludesStatusCancelled()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var item = await CreatePublishedEventAsync(accessToken, eventTypeId, NewScheduleRequest());

        var scheduleResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{item.Id}/event", accessToken));
        var schedule = await scheduleResponse.Content.ReadFromJsonAsync<EventScheduleDetailResponse>();
        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{item.Id}/event/cancel", accessToken,
            new GenclikMerkezi.Modules.Website.Features.CancelEventSchedule.CancelEventScheduleRequest(schedule!.RowVersion, "Test")));

        var response = await _client.GetAsync($"/api/v1/public/events/{item.Id}/calendar.ics");
        var ics = await response.Content.ReadAsStringAsync();

        Assert.Contains("STATUS:CANCELLED", ics);
    }
}
