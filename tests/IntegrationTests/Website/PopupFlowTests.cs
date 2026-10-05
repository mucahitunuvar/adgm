using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Application.Popups;
using GenclikMerkezi.Modules.Website.Features.ActivatePopup;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.CreatePopup;
using GenclikMerkezi.Modules.Website.Features.DeactivatePopup;
using GenclikMerkezi.Modules.Website.Features.DeletePopupTranslation;
using GenclikMerkezi.Modules.Website.Features.GetContentItemById;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.GetPopupById;
using GenclikMerkezi.Modules.Website.Features.GetPopups;
using GenclikMerkezi.Modules.Website.Features.PublishContentItem;
using GenclikMerkezi.Modules.Website.Features.UpdatePopup;
using GenclikMerkezi.Modules.Website.Features.UpdatePopupTranslation;
using GenclikMerkezi.Modules.Website.Features.UploadMediaAsset;
using SkiaSharp;

namespace GenclikMerkezi.IntegrationTests.Website;

// Faz 2 Görev 6 master prompt §6. Shares the "one CustomWebApplicationFactory/one Sqlite database per
// class" caveat every other Website flow test class documents - a real database and real HTTP calls
// exercise both the admin CRUD/validation rules and the public site response's `popups` field
// end-to-end (scheduling, language and Contents-targeting-to-paths resolution are the three the task
// explicitly calls out for integration coverage; PopupTests/PopupTargetingTests already unit-cover
// every other domain rule in isolation).
public class PopupFlowTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly List<Guid> _createdPopupIds = [];

    public PopupFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    // Popup.Create's "en fazla 20 aktif ve süresi dolmamış pop-up" limit is a database-wide invariant,
    // and this class shares one database across every test method (xUnit still gives each test its
    // own PopupFlowTests instance, so this runs once per test) - without cleanup, popups any earlier
    // test left active would eventually push later tests (including ones that aren't testing the
    // limit at all) over that limit.
    public async Task DisposeAsync()
    {
        if (_createdPopupIds.Count == 0)
        {
            return;
        }

        var accessToken = await LoginAsAdminAsync();
        foreach (var id in _createdPopupIds)
        {
            await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/popups/{id}", accessToken));
        }
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

    private async Task<string> LoginAsNonAdminAsync()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email, password = "Sifre123", firstName = "Test", lastName = "User", role = "Candidate" });
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Sifre123" });
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

    private static readonly PopupTargetingInput AllPages = new("AllPages", null, null);
    private static readonly LinkTargetDto NoLink = new("None", null, null, null, null);

    private static LinkTargetDto ExternalLink(string url) => new("ExternalUrl", null, null, null, url);

    // --- Popup CRUD plumbing ---

    private async Task<HttpResponseMessage> CreatePopupRawAsync(CreatePopupRequest request, string accessToken) =>
        await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/popups", accessToken, request));

    private async Task<Guid> CreatePopupAsync(string accessToken, CreatePopupRequest request)
    {
        var response = await CreatePopupRawAsync(request, accessToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreatePopupResponse>();
        _createdPopupIds.Add(created!.Id);
        return created.Id;
    }

    private static CreatePopupRequest ModalRequest(
        string title = "Başlık",
        string body = "<p>İçerik</p>",
        LinkTargetDto? link = null,
        string? buttonLabel = null,
        PopupTargetingInput? targeting = null,
        int priority = 50,
        DateTime? publishAtUtc = null,
        DateTime? unpublishAtUtc = null,
        Guid? imageMediaId = null) =>
        new(
            "Modal", imageMediaId, link ?? NoLink, targeting ?? AllPages, "All", publishAtUtc, unpublishAtUtc, 0, "EveryVisit", null, true,
            priority, title, body, buttonLabel);

    private static CreatePopupRequest BannerRequest(
        string body = "Kısa metin",
        LinkTargetDto? link = null,
        string? buttonLabel = null,
        PopupTargetingInput? targeting = null,
        int priority = 50,
        bool dismissible = true) =>
        new("Banner", null, link ?? NoLink, targeting ?? AllPages, "All", null, null, 0, "EveryVisit", null, dismissible, priority, null, body, buttonLabel);

    private async Task<PopupDetailResponse> GetPopupAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/popups/{id}", accessToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<PopupDetailResponse>())!;
    }

    // --- Media / content item plumbing ---

    private static byte[] CreateJpeg(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.CornflowerBlue);
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        return data.ToArray();
    }

    private async Task<Guid> UploadImageAsync(string accessToken)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(CreateJpeg(400, 300));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "popup.jpg");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/website/media") { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var uploaded = await response.Content.ReadFromJsonAsync<UploadMediaAssetResponse>();
        return uploaded!.Id;
    }

    // "en" is seeded inactive from day one (see GenclikMerkeziContentTypeSeed's own remarks) - a
    // request for ?lang=en against an inactive language silently falls back to the default ("tr"),
    // so tests that need to prove a popup is excluded/included FOR "en" specifically must activate it
    // first. Idempotent (SiteLanguage.Activate is a plain IsActive = true, no error if already active).
    private async Task EnsureEnglishActiveAsync(string accessToken)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/languages", accessToken));
        var languages = await response.Content.ReadFromJsonAsync<GetSiteLanguagesOnlyResponse>();
        var english = languages!.Items.Single(l => l.Code == "en");
        if (!english.IsActive)
        {
            await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/v1/admin/website/languages/{english.Id}/activate", accessToken));
        }
    }

    private sealed record GetSiteLanguagesOnlyResponse(List<SiteLanguageOnlyResponse> Items);

    private sealed record SiteLanguageOnlyResponse(Guid Id, string Code, bool IsActive);

    private async Task<Guid> GetNewsContentTypeIdAsync(string accessToken)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/content-types", accessToken));
        var types = await response.Content.ReadFromJsonAsync<List<ContentTypeSummaryResponse>>();
        return types!.Single(t => t.Key == "news").Id;
    }

    private async Task<ContentItemDetailResponse> GetContentItemAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{id}", accessToken));
        return (await response.Content.ReadFromJsonAsync<ContentItemDetailResponse>())!;
    }

    private async Task<Guid> CreateNewsItemAsync(string accessToken, string title, bool publish)
    {
        var slug = $"haber-{Guid.NewGuid():N}"[..24];
        var createRequest = new CreateContentItemRequest(
            await GetNewsContentTypeIdAsync(accessToken), null, 1, false, null, null, title, slug, "Özet", "<p>Gövde</p>",
            new CreateContentItemSeoInput(null, null, null, null, null, null, null, false));
        var createResponse = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, createRequest));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = (await createResponse.Content.ReadFromJsonAsync<CreateContentItemResponse>())!;

        if (publish)
        {
            var detail = await GetContentItemAsync(accessToken, created.Id);
            var publishResponse = await _client.SendAsync(Authorized(
                HttpMethod.Post, $"/api/v1/admin/website/contents/{created.Id}/publish", accessToken,
                new PublishContentItemRequest(detail.RowVersion, null, null)));
            Assert.Equal(HttpStatusCode.NoContent, publishResponse.StatusCode);
        }

        return created.Id;
    }

    // --- Public site plumbing ---

    private async Task<List<PublicPopupOnlyResponse>> GetPublicPopupsAsync(string? lang = null)
    {
        var response = await _client.GetAsync("/api/v1/public/site" + (lang is null ? string.Empty : $"?lang={lang}"));
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new Exception($"Expected OK, got {response.StatusCode}: {errorBody}");
        }
        var body = (await response.Content.ReadFromJsonAsync<PublicSitePopupsOnlyResponse>())!;
        return body.Popups;
    }

    private sealed record PublicSitePopupsOnlyResponse(List<PublicPopupOnlyResponse> Popups);

    private sealed record PublicPopupOnlyResponse(
        Guid Id, string DisplayMode, string? Title, string Body, string? ButtonLabel, string? Href,
        PublicPopupTargetingOnlyResponse Targeting, int Priority);

    private sealed record PublicPopupTargetingOnlyResponse(string Kind, List<string> Paths);

    // --- 1. Mode-based validation ---

    [Fact]
    public async Task CreatePopup_Banner_WithImage_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var imageId = await UploadImageAsync(accessToken);

        var response = await CreatePopupRawAsync(BannerRequest() with { ImageMediaId = imageId }, accessToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePopup_Banner_WithHtmlBody_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();

        var response = await CreatePopupRawAsync(BannerRequest(body: "<b>metin</b>"), accessToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePopup_Modal_WithValidInput_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var imageId = await UploadImageAsync(accessToken);

        var id = await CreatePopupAsync(accessToken, ModalRequest(imageMediaId: imageId));

        var popup = await GetPopupAsync(accessToken, id);
        Assert.Equal("Modal", popup.DisplayMode);
        Assert.True(popup.Dismissible);
        Assert.NotNull(popup.Image);
    }

    [Fact]
    public async Task CreatePopup_EveryNDaysWithoutFrequencyDays_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var request = ModalRequest() with { Frequency = "EveryNDays" };

        var response = await CreatePopupRawAsync(request, accessToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePopup_ButtonLabelWithoutLink_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();

        var response = await CreatePopupRawAsync(ModalRequest(buttonLabel: "Git"), accessToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // --- 2. Targeting validation ---

    [Fact]
    public async Task CreatePopup_Targeting_PathWithLanguagePrefix_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var targeting = new PopupTargetingInput("Paths", null, ["/tr/haberler"]);

        var response = await CreatePopupRawAsync(ModalRequest(targeting: targeting), accessToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePopup_Targeting_WildcardPath_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var targeting = new PopupTargetingInput("Paths", null, ["/haberler/*"]);

        var id = await CreatePopupAsync(accessToken, ModalRequest(targeting: targeting));

        var popup = await GetPopupAsync(accessToken, id);
        Assert.Equal("Paths", popup.Targeting.Kind);
        Assert.Contains("/haberler/*", popup.Targeting.Paths);
    }

    [Fact]
    public async Task CreatePopup_Targeting_MissingContentItem_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var targeting = new PopupTargetingInput("Contents", [Guid.NewGuid()], null);

        var response = await CreatePopupRawAsync(ModalRequest(targeting: targeting), accessToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- 3. Active popup limit ---

    [Fact]
    public async Task CreatePopup_TwentyFirstActivePopup_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        for (var i = 0; i < 20; i++)
        {
            await CreatePopupAsync(accessToken, ModalRequest(title: $"Popup {i}-{Guid.NewGuid():N}"));
        }

        var response = await CreatePopupRawAsync(ModalRequest(title: $"Popup extra-{Guid.NewGuid():N}"), accessToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // --- 4. Admin authorization ---

    [Fact]
    public async Task GetPopups_AsNonAdmin_ReturnsForbidden()
    {
        var accessToken = await LoginAsNonAdminAsync();

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/popups", accessToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // --- 5. Media usage protection ---

    [Fact]
    public async Task DeleteMediaAsset_UsedByPopupImage_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var imageId = await UploadImageAsync(accessToken);
        await CreatePopupAsync(accessToken, ModalRequest(imageMediaId: imageId));

        var response = await _client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/admin/website/media/{imageId}", accessToken));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // --- 6. Public site: visibility, scheduling, language ---

    [Fact]
    public async Task GetPublicSite_ReturnsVisiblePopupOrderedByPriority()
    {
        var accessToken = await LoginAsAdminAsync();
        var marker = Guid.NewGuid().ToString("N");
        await CreatePopupAsync(accessToken, ModalRequest(title: $"Düşük-{marker}", priority: 10));
        await CreatePopupAsync(accessToken, ModalRequest(title: $"Yüksek-{marker}", priority: 90));

        var popups = await GetPublicPopupsAsync("tr");
        var ours = popups.Where(p => p.Title != null && p.Title.Contains(marker)).ToList();

        Assert.Equal(2, ours.Count);
        Assert.Equal($"Yüksek-{marker}", ours[0].Title);
        Assert.Equal($"Düşük-{marker}", ours[1].Title);
    }

    [Fact]
    public async Task GetPublicSite_ExcludesPopupScheduledInTheFuture()
    {
        var accessToken = await LoginAsAdminAsync();
        var marker = Guid.NewGuid().ToString("N");
        await CreatePopupAsync(
            accessToken, ModalRequest(title: $"Gelecek-{marker}", publishAtUtc: DateTime.UtcNow.AddDays(1)));

        var popups = await GetPublicPopupsAsync("tr");

        Assert.DoesNotContain(popups, p => p.Title != null && p.Title.Contains(marker));
    }

    [Fact]
    public async Task GetPublicSite_ExcludesExpiredPopup()
    {
        var accessToken = await LoginAsAdminAsync();
        var marker = Guid.NewGuid().ToString("N");
        await CreatePopupAsync(
            accessToken, ModalRequest(title: $"Süresi-{marker}", unpublishAtUtc: DateTime.UtcNow.AddMinutes(-1)));

        var popups = await GetPublicPopupsAsync("tr");

        Assert.DoesNotContain(popups, p => p.Title != null && p.Title.Contains(marker));
    }

    [Fact]
    public async Task GetPublicSite_ExcludesInactivePopup()
    {
        var accessToken = await LoginAsAdminAsync();
        var marker = Guid.NewGuid().ToString("N");
        var id = await CreatePopupAsync(accessToken, ModalRequest(title: $"Pasif-{marker}"));
        var popup = await GetPopupAsync(accessToken, id);
        var deactivateResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/popups/{id}/deactivate", accessToken, new DeactivatePopupRequest(popup.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, deactivateResponse.StatusCode);

        var popups = await GetPublicPopupsAsync("tr");

        Assert.DoesNotContain(popups, p => p.Title != null && p.Title.Contains(marker));
    }

    [Fact]
    public async Task ActivatePopup_AfterDeactivate_ReappearsOnPublicSite()
    {
        var accessToken = await LoginAsAdminAsync();
        var marker = Guid.NewGuid().ToString("N");
        var id = await CreatePopupAsync(accessToken, ModalRequest(title: $"Yeniden-{marker}"));
        var afterCreate = await GetPopupAsync(accessToken, id);
        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/popups/{id}/deactivate", accessToken, new DeactivatePopupRequest(afterCreate.RowVersion)));

        var afterDeactivate = await GetPopupAsync(accessToken, id);
        var activateResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/popups/{id}/activate", accessToken, new ActivatePopupRequest(afterDeactivate.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, activateResponse.StatusCode);

        var popups = await GetPublicPopupsAsync("tr");

        Assert.Contains(popups, p => p.Title != null && p.Title.Contains(marker));
    }

    [Fact]
    public async Task GetPublicSite_ExcludesPopupWithoutTranslationInRequestedLanguage()
    {
        var accessToken = await LoginAsAdminAsync();
        await EnsureEnglishActiveAsync(accessToken);
        var marker = Guid.NewGuid().ToString("N");
        // Default language is "tr" (seeded) - a popup only ever has a "tr" translation here.
        await CreatePopupAsync(accessToken, ModalRequest(title: $"SadeceTr-{marker}"));

        var popupsInTurkish = await GetPublicPopupsAsync("tr");
        Assert.Contains(popupsInTurkish, p => p.Title != null && p.Title.Contains(marker));

        var popupsInEnglish = await GetPublicPopupsAsync("en");
        Assert.DoesNotContain(popupsInEnglish, p => p.Title != null && p.Title.Contains(marker));
    }

    [Fact]
    public async Task UpdatePopupTranslation_AddsSecondLanguageTranslation_ThenVisibleInThatLanguage()
    {
        var accessToken = await LoginAsAdminAsync();
        await EnsureEnglishActiveAsync(accessToken);
        var marker = Guid.NewGuid().ToString("N");
        var id = await CreatePopupAsync(accessToken, ModalRequest(title: $"Yalnizca-Tr-{marker}"));
        var popup = await GetPopupAsync(accessToken, id);

        var translationResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/popups/{id}/translations/en", accessToken,
            new UpdatePopupTranslationRequest(popup.RowVersion, $"Multi-{marker}", "<p>Content</p>", null)));
        Assert.Equal(HttpStatusCode.NoContent, translationResponse.StatusCode);

        var popupsInEnglish = await GetPublicPopupsAsync("en");
        Assert.Contains(popupsInEnglish, p => p.Title != null && p.Title.Contains($"Multi-{marker}"));
    }

    [Fact]
    public async Task DeletePopupTranslation_RemovesItFromThatLanguagesPublicResponse()
    {
        var accessToken = await LoginAsAdminAsync();
        await EnsureEnglishActiveAsync(accessToken);
        var marker = Guid.NewGuid().ToString("N");
        var id = await CreatePopupAsync(accessToken, ModalRequest(title: $"Silinecek-Tr-{marker}"));
        var popup = await GetPopupAsync(accessToken, id);
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/popups/{id}/translations/en", accessToken,
            new UpdatePopupTranslationRequest(popup.RowVersion, $"Silinecek-En-{marker}", "<p>Content</p>", null)));

        var afterAdd = await GetPopupAsync(accessToken, id);
        var deleteResponse = await _client.SendAsync(Authorized(
            HttpMethod.Delete, $"/api/v1/admin/website/popups/{id}/translations/en", accessToken,
            new DeletePopupTranslationRequest(afterAdd.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var popupsInEnglish = await GetPublicPopupsAsync("en");
        Assert.DoesNotContain(popupsInEnglish, p => p.Title != null && p.Title.Contains($"Silinecek-En-{marker}"));
    }

    // --- 7. Public site: link resolution ---

    [Fact]
    public async Task GetPublicSite_ButtonHiddenWhenLinkDoesNotResolve()
    {
        var accessToken = await LoginAsAdminAsync();
        var marker = Guid.NewGuid().ToString("N");
        var missingContentLink = new LinkTargetDto("Content", Guid.NewGuid(), null, null, null);
        await CreatePopupAsync(
            accessToken, ModalRequest(title: $"KirikLink-{marker}", link: missingContentLink, buttonLabel: "Git"));

        var popups = await GetPublicPopupsAsync("tr");
        var popup = Assert.Single(popups, p => p.Title != null && p.Title.Contains(marker));

        Assert.Null(popup.Href);
        Assert.Null(popup.ButtonLabel);
    }

    [Fact]
    public async Task GetPublicSite_ButtonVisibleWhenLinkResolves()
    {
        var accessToken = await LoginAsAdminAsync();
        var marker = Guid.NewGuid().ToString("N");
        await CreatePopupAsync(
            accessToken,
            ModalRequest(title: $"CalisanLink-{marker}", link: ExternalLink("https://example.com/kampanya"), buttonLabel: "Git"));

        var popups = await GetPublicPopupsAsync("tr");
        var popup = Assert.Single(popups, p => p.Title != null && p.Title.Contains(marker));

        Assert.Equal("https://example.com/kampanya", popup.Href);
        Assert.Equal("Git", popup.ButtonLabel);
    }

    // --- 8. Public site: Contents targeting resolved to paths ---

    [Fact]
    public async Task GetPublicSite_ContentsTargeting_ResolvesToPathsAndDropsInvisibleContent()
    {
        var accessToken = await LoginAsAdminAsync();
        var marker = Guid.NewGuid().ToString("N");
        var publishedId = await CreateNewsItemAsync(accessToken, $"Yayinda-{marker}", publish: true);
        var publishedItem = await GetContentItemAsync(accessToken, publishedId);
        var draftId = await CreateNewsItemAsync(accessToken, $"Taslak-{marker}", publish: false);

        var targeting = new PopupTargetingInput("Contents", [publishedId, draftId], null);
        await CreatePopupAsync(accessToken, ModalRequest(title: $"HedefliPopup-{marker}", targeting: targeting));

        var popups = await GetPublicPopupsAsync("tr");
        var popup = Assert.Single(popups, p => p.Title != null && p.Title.Contains(marker));

        var publishedFullPath = publishedItem.Translations.First(t => t.LanguageCode == "tr").FullPath;
        Assert.Equal("Contents", popup.Targeting.Kind);
        var path = Assert.Single(popup.Targeting.Paths);
        Assert.EndsWith(publishedFullPath, path, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetPublicSite_PathsTargeting_PassesThroughUnchanged()
    {
        var accessToken = await LoginAsAdminAsync();
        var marker = Guid.NewGuid().ToString("N");
        var targeting = new PopupTargetingInput("Paths", null, ["/kampanya/*", "/iletisim"]);
        await CreatePopupAsync(accessToken, ModalRequest(title: $"YolHedefli-{marker}", targeting: targeting));

        var popups = await GetPublicPopupsAsync("tr");
        var popup = Assert.Single(popups, p => p.Title != null && p.Title.Contains(marker));

        Assert.Equal("Paths", popup.Targeting.Kind);
        Assert.Equal(["/kampanya/*", "/iletisim"], popup.Targeting.Paths);
    }

    // --- 9. Concurrency ---

    [Fact]
    public async Task UpdatePopup_WithStaleRowVersion_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var id = await CreatePopupAsync(accessToken, ModalRequest());
        var initial = await GetPopupAsync(accessToken, id);

        var firstUpdate = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/popups/{id}", accessToken,
            new UpdatePopupRequest(
                initial.RowVersion, "Modal", null, NoLink, AllPages, "All", null, null, 0, "EveryVisit", null, true, 60)));
        Assert.Equal(HttpStatusCode.NoContent, firstUpdate.StatusCode);

        var secondUpdate = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/popups/{id}", accessToken,
            new UpdatePopupRequest(
                initial.RowVersion, "Modal", null, NoLink, AllPages, "All", null, null, 0, "EveryVisit", null, true, 70)));

        Assert.Equal(HttpStatusCode.Conflict, secondUpdate.StatusCode);
    }
}
