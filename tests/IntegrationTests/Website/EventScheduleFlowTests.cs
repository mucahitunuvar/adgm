using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.CancelEventSchedule;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.CreateContentType;
using GenclikMerkezi.Modules.Website.Features.DuplicateContentItem;
using GenclikMerkezi.Modules.Website.Features.GetContentTypeById;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.GetEventScheduleByContentItemId;
using GenclikMerkezi.Modules.Website.Features.ReactivateEventSchedule;
using GenclikMerkezi.Modules.Website.Features.UpdateContentType;
using GenclikMerkezi.Modules.Website.Features.UpsertEventSchedule;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §11.1 (Faz 4 Görev 1). Uses the "event" ContentType seeded by GenclikMerkeziContentTypeSeed
// (SupportsEvent + EventDateAsc) for the happy-path/negative tests that only need an existing
// SupportsEvent type, and a freshly created ContentType for the one test that mutates SupportsEvent
// itself - never the shared seed row, so other tests' assumptions about "event"/"training" stay intact.
public class EventScheduleFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly CreateContentItemSeoInput EmptySeo = new(null, null, null, null, null, null, null, false);
    private static readonly CreateContentTypeSeoInput EmptyTypeSeo = new(null, null, null, null, null, null, null, false);

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EventScheduleFlowTests(CustomWebApplicationFactory factory)
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

    private static UpsertEventScheduleRequest NewScheduleRequest(
        byte[]? rowVersion = null,
        DateTime? startsAtUtc = null,
        DateTime? endsAtUtc = null,
        string format = "InPerson",
        string? onlineLink = null,
        int? capacity = 10,
        bool autoConfirm = true,
        bool waitlistEnabled = true,
        string venueName = "Gençlik Merkezi Salonu") =>
        new(
            rowVersion, startsAtUtc ?? DateTime.UtcNow.AddDays(10), endsAtUtc ?? DateTime.UtcNow.AddDays(10).AddHours(2), format, onlineLink,
            capacity, true, null, null, null, null, autoConfirm, waitlistEnabled,
            [new UpsertEventScheduleTranslationInput("tr", venueName, "Örnek Adres No:1", "Ücretsiz", "Eğitmen", "<p>Program</p>", "Not")]);

    [Fact]
    public async Task UpsertEventSchedule_OnContentTypeWithoutSupportsEvent_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var pageTypeId = await GetContentTypeIdAsync(accessToken, "page");
        var contentItemId = await CreateContentItemAsync(accessToken, pageTypeId, $"Sayfa {Guid.NewGuid():N}");

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken, NewScheduleRequest()));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpsertEventSchedule_Create_ThenGet_ReturnsTheSchedule()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreateContentItemAsync(accessToken, eventTypeId, $"Etkinlik {Guid.NewGuid():N}");

        var createResponse = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken, NewScheduleRequest()));

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<EventScheduleDetailResponse>();
        Assert.Equal(contentItemId, created!.ContentItemId);
        Assert.Equal(0, created.ConfirmedCount);
        Assert.False(created.IsCancelled);

        var getResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken));
        var fetched = await getResponse.Content.ReadFromJsonAsync<EventScheduleDetailResponse>();
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal("Gençlik Merkezi Salonu", fetched!.Translations[0].VenueName);
    }

    [Fact]
    public async Task UpsertEventSchedule_WithoutDefaultLanguageTranslation_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreateContentItemAsync(accessToken, eventTypeId, $"Etkinlik {Guid.NewGuid():N}");
        var request = NewScheduleRequest() with
        {
            Translations = [new UpsertEventScheduleTranslationInput("en", "Hall", "Address", "Free", "Instructor", "<p/>", "Note")],
        };

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken, request));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("Online")]
    [InlineData("Hybrid")]
    public async Task UpsertEventSchedule_OnlineOrHybridWithoutOnlineLink_ReturnsBadRequest(string format)
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreateContentItemAsync(accessToken, eventTypeId, $"Etkinlik {Guid.NewGuid():N}");
        var request = NewScheduleRequest(format: format, onlineLink: null);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken, request));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpsertEventSchedule_Update_WithStaleRowVersion_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreateContentItemAsync(accessToken, eventTypeId, $"Etkinlik {Guid.NewGuid():N}");
        var createResponse = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken, NewScheduleRequest()));
        var created = await createResponse.Content.ReadFromJsonAsync<EventScheduleDetailResponse>();

        var firstUpdate = NewScheduleRequest(rowVersion: created!.RowVersion, capacity: 20);
        var firstResponse = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken, firstUpdate));
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var secondResponse = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken, firstUpdate));

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task UpsertEventSchedule_Update_AfterEventStarted_WithChangedCapacity_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreateContentItemAsync(accessToken, eventTypeId, $"Etkinlik {Guid.NewGuid():N}");
        var startsAtUtc = DateTime.UtcNow.AddSeconds(-5);
        var createResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Put, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken,
                NewScheduleRequest(startsAtUtc: startsAtUtc, endsAtUtc: startsAtUtc.AddHours(2))));
        var created = await createResponse.Content.ReadFromJsonAsync<EventScheduleDetailResponse>();

        var update = NewScheduleRequest(
            rowVersion: created!.RowVersion, startsAtUtc: startsAtUtc, endsAtUtc: startsAtUtc.AddHours(2), capacity: 999);
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken, update));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CancelEventSchedule_ThenReactivate_RoundTrips()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreateContentItemAsync(accessToken, eventTypeId, $"Etkinlik {Guid.NewGuid():N}");
        var createResponse = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken, NewScheduleRequest()));
        var created = await createResponse.Content.ReadFromJsonAsync<EventScheduleDetailResponse>();

        var cancelResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Post, $"/api/v1/admin/website/contents/{contentItemId}/event/cancel", accessToken,
                new CancelEventScheduleRequest(created!.RowVersion, "Hava koşulları")));
        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

        var afterCancelResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken));
        var afterCancel = await afterCancelResponse.Content.ReadFromJsonAsync<EventScheduleDetailResponse>();
        Assert.True(afterCancel!.IsCancelled);
        Assert.Equal("Hava koşulları", afterCancel.CancellationReason);

        var reactivateResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Post, $"/api/v1/admin/website/contents/{contentItemId}/event/reactivate", accessToken,
                new ReactivateEventScheduleRequest(afterCancel.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, reactivateResponse.StatusCode);

        var finalResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken));
        var final = await finalResponse.Content.ReadFromJsonAsync<EventScheduleDetailResponse>();
        Assert.False(final!.IsCancelled);
    }

    [Fact]
    public async Task DeleteEventSchedule_RemovesIt_AndSubsequentGetReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreateContentItemAsync(accessToken, eventTypeId, $"Etkinlik {Guid.NewGuid():N}");
        await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken, NewScheduleRequest()));

        var deleteResponse = await _client.SendAsync(
            Authorized(HttpMethod.Delete, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken));
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DuplicateContentItem_WithEventSchedule_CopiesScheduleWithRegistrationDisabledAndCountersReset()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdAsync(accessToken, "event");
        var contentItemId = await CreateContentItemAsync(accessToken, eventTypeId, $"Etkinlik {Guid.NewGuid():N}");
        await _client.SendAsync(
            Authorized(
                HttpMethod.Put, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken,
                NewScheduleRequest(venueName: "Orijinal Salon")));

        var duplicateResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/admin/website/contents/{contentItemId}/duplicate", accessToken));
        var duplicated = await duplicateResponse.Content.ReadFromJsonAsync<DuplicateContentItemResponse>();

        var copiedScheduleResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{duplicated!.Id}/event", accessToken));
        Assert.Equal(HttpStatusCode.OK, copiedScheduleResponse.StatusCode);
        var copiedSchedule = await copiedScheduleResponse.Content.ReadFromJsonAsync<EventScheduleDetailResponse>();
        Assert.False(copiedSchedule!.RegistrationEnabled);
        Assert.False(copiedSchedule.IsCancelled);
        Assert.Equal(0, copiedSchedule.ConfirmedCount);
        Assert.Equal(0, copiedSchedule.WaitlistedCount);
        Assert.Equal("Orijinal Salon", copiedSchedule.Translations[0].VenueName);
    }

    [Fact]
    public async Task UpdateContentType_DisablingSupportsEventWhileScheduleExists_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = $"test-event-{Guid.NewGuid():N}"[..20];
        var createTypeRequest = new CreateContentTypeRequest(
            key, "cards", "event", "EventDateAsc", 99,
            SupportsHierarchy: false, SupportsCategories: false, SupportsTags: false, SupportsDetailImage: false,
            SupportsGallery: false, SupportsVideos: false, SupportsAttachments: false, SupportsEvent: true,
            SupportsBlockLayout: false, SupportsForm: false, SupportsRelatedContent: false, HasDetailPage: true,
            HasListingPage: true, IsSearchable: true, RequiresReview: false,
            DefaultLanguageName: "Test Etkinlik Türü", DefaultLanguageRoutePrefix: key, Seo: EmptyTypeSeo);
        var createTypeResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, "/api/v1/admin/website/content-types", accessToken, createTypeRequest));
        var createdType = await createTypeResponse.Content.ReadFromJsonAsync<CreateContentTypeResponse>();

        var contentItemId = await CreateContentItemAsync(accessToken, createdType!.Id, $"Etkinlik {Guid.NewGuid():N}");
        await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/contents/{contentItemId}/event", accessToken, NewScheduleRequest()));

        var typeDetailResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/content-types/{createdType.Id}", accessToken));
        var typeDetail = await typeDetailResponse.Content.ReadFromJsonAsync<ContentTypeDetailResponse>();

        var updateTypeRequest = new UpdateContentTypeRequest(
            typeDetail!.RowVersion, "cards", "event", "EventDateAsc", 99,
            SupportsHierarchy: false, SupportsCategories: false, SupportsTags: false, SupportsDetailImage: false,
            SupportsGallery: false, SupportsVideos: false, SupportsAttachments: false, SupportsEvent: false,
            SupportsBlockLayout: false, SupportsForm: false, SupportsRelatedContent: false, HasDetailPage: true,
            HasListingPage: true, IsSearchable: true, RequiresReview: false);
        var updateResponse = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/content-types/{createdType.Id}", accessToken, updateTypeRequest));

        Assert.Equal(HttpStatusCode.Conflict, updateResponse.StatusCode);
    }

    [Fact]
    public async Task GetEventScheduleByContentItemId_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync($"/api/v1/admin/website/contents/{Guid.NewGuid()}/event");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
