using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.CreateContentType;
using GenclikMerkezi.Modules.Website.Features.GetContentPreview;
using GenclikMerkezi.Modules.Website.Features.GetContentTypeById;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.GetPublicContentById;
using GenclikMerkezi.Modules.Website.Features.PublishContentItem;
using GenclikMerkezi.Modules.Website.Features.UpdateContentType;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsContact;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsIdentity;
using GenclikMerkezi.Modules.Website.Features.UpsertEventSchedule;
using SkiaSharp;

namespace GenclikMerkezi.IntegrationTests.Website;

// SiteSettings is a singleton row shared by every test running against the same
// CustomWebApplicationFactory instance (ADR-024 §13) - this "before any update" assertion lives in
// its own class, the same split SiteSettingsFlowTests.cs uses for SiteSettingsDefaultsTests, so
// nothing in StructuredDataFlowTests (which mutates the singleton's identity/contact groups) can
// race it.
public class OrganizationDefaultsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrganizationDefaultsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPublicSite_WithNoSettingsConfigured_OrganizationOmitsEveryOptionalField()
    {
        var response = await _client.GetAsync("/api/v1/public/site");
        var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        var organization = root.GetProperty("organization");
        Assert.Equal("Organization", organization.GetProperty("@type").GetString());
        Assert.False(organization.TryGetProperty("logo", out _));
        Assert.False(organization.TryGetProperty("sameAs", out _));
        Assert.False(organization.TryGetProperty("contactPoint", out _));
        Assert.False(organization.TryGetProperty("address", out _));
    }
}

// ADR-024 §15 (Faz 5 Görev 6): schema.org structured data - ContentType.SchemaKind CRUD/seed,
// jsonLd on the public detail/list endpoints, and the public site's organization field. Shares the
// "one CustomWebApplicationFactory/one Sqlite database per class" caveat every other Website flow
// test class documents; reuses the seeded "news" (NewsArticle), "project" (Article), "faq" (FaqPage),
// "page" (None) and "event" (SupportsEvent, SchemaKind None) content types.
public class StructuredDataFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly CreateContentItemSeoInput EmptySeo = new(null, null, null, null, null, null, null, false);
    private static readonly CreateContentTypeSeoInput EmptyTypeSeo = new(null, null, null, null, null, null, null, false);

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public StructuredDataFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<Guid> GetContentTypeIdByKeyAsync(string accessToken, string key)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/content-types", accessToken));
        var types = await response.Content.ReadFromJsonAsync<List<ContentTypeSummaryResponse>>();
        return types!.Single(t => t.Key == key).Id;
    }

    private async Task<string> GetContentTypeSchemaKindAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/content-types/{id}", accessToken));
        var body = await response.Content.ReadFromJsonAsync<ContentTypeDetailResponse>();
        return body!.SchemaKind;
    }

    private async Task<(Guid Id, string FullPath)> CreateContentItemAsync(
        string accessToken, Guid contentTypeId, string title, string? body = null, string? summary = null)
    {
        var slug = $"slug-{Guid.NewGuid():N}";
        var request = new CreateContentItemRequest(contentTypeId, null, 1, false, null, null, title, slug, summary, body, EmptySeo);
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, request));
        var created = await response.Content.ReadFromJsonAsync<CreateContentItemResponse>();
        return (created!.Id, created.FullPath);
    }

    private async Task PublishAsync(string accessToken, Guid id)
    {
        var getResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{id}", accessToken));
        var rowVersion = (await getResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("rowVersion").Deserialize<byte[]>();
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{id}/publish", accessToken, new PublishContentItemRequest(rowVersion, null, null)));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private static async Task<JsonElement> ParseAsync(HttpResponseMessage response) =>
        JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

    // --- ContentType.SchemaKind: seed + CRUD ---

    [Theory]
    [InlineData("news", "NewsArticle")]
    [InlineData("announcement", "NewsArticle")]
    [InlineData("press-release", "NewsArticle")]
    [InlineData("project", "Article")]
    [InlineData("activity", "Article")]
    [InlineData("success-story", "Article")]
    [InlineData("faq", "FaqPage")]
    [InlineData("page", "None")]
    [InlineData("event", "None")]
    [InlineData("team", "None")]
    public async Task GetContentTypeById_SeedType_HasExpectedSchemaKind(string key, string expectedSchemaKind)
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await GetContentTypeIdByKeyAsync(accessToken, key);

        var schemaKind = await GetContentTypeSchemaKindAsync(accessToken, typeId);

        Assert.Equal(expectedSchemaKind, schemaKind);
    }

    [Fact]
    public async Task CreateContentType_WithSchemaKind_ThenGetById_ReturnsIt()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = $"ozel-tur-{Guid.NewGuid():N}";
        var request = new CreateContentTypeRequest(
            key, "list", "article", "PublishDateDesc", 99,
            SupportsHierarchy: false, SupportsCategories: false, SupportsTags: false, SupportsDetailImage: false,
            SupportsGallery: false, SupportsVideos: false, SupportsAttachments: false, SupportsEvent: false,
            SupportsBlockLayout: false, SupportsForm: false, SupportsRelatedContent: false, HasDetailPage: true,
            HasListingPage: true, IsSearchable: true, RequiresReview: false,
            DefaultLanguageName: "Özel Tür", DefaultLanguageRoutePrefix: key, Seo: EmptyTypeSeo, SchemaKind: "Article");

        var createResponse = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/content-types", accessToken, request));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateContentTypeResponse>();

        var schemaKind = await GetContentTypeSchemaKindAsync(accessToken, created!.Id);
        Assert.Equal("Article", schemaKind);
    }

    [Fact]
    public async Task UpdateContentType_ChangingSchemaKind_ReflectsOnNextRead()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = $"ozel-tur-{Guid.NewGuid():N}";
        var createRequest = new CreateContentTypeRequest(
            key, "list", "article", "PublishDateDesc", 99,
            SupportsHierarchy: false, SupportsCategories: false, SupportsTags: false, SupportsDetailImage: false,
            SupportsGallery: false, SupportsVideos: false, SupportsAttachments: false, SupportsEvent: false,
            SupportsBlockLayout: false, SupportsForm: false, SupportsRelatedContent: false, HasDetailPage: true,
            HasListingPage: true, IsSearchable: true, RequiresReview: false,
            DefaultLanguageName: "Özel Tür", DefaultLanguageRoutePrefix: key, Seo: EmptyTypeSeo, SchemaKind: "None");
        var createResponse = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/content-types", accessToken, createRequest));
        var created = await createResponse.Content.ReadFromJsonAsync<CreateContentTypeResponse>();

        var detail = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/content-types/{created!.Id}", accessToken));
        var detailBody = await detail.Content.ReadFromJsonAsync<ContentTypeDetailResponse>();

        var updateRequest = new UpdateContentTypeRequest(
            detailBody!.RowVersion, "list", "article", "PublishDateDesc", 99,
            SupportsHierarchy: false, SupportsCategories: false, SupportsTags: false, SupportsDetailImage: false,
            SupportsGallery: false, SupportsVideos: false, SupportsAttachments: false, SupportsEvent: false,
            SupportsBlockLayout: false, SupportsForm: false, SupportsRelatedContent: false, HasDetailPage: true,
            HasListingPage: true, IsSearchable: true, RequiresReview: false, SchemaKind: "NewsArticle");
        var updateResponse = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/content-types/{created.Id}", accessToken, updateRequest));
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var schemaKind = await GetContentTypeSchemaKindAsync(accessToken, created.Id);
        Assert.Equal("NewsArticle", schemaKind);
    }

    [Fact]
    public async Task UpdateContentType_WithUnrecognizedSchemaKind_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await GetContentTypeIdByKeyAsync(accessToken, "page");
        var detail = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/content-types/{typeId}", accessToken));
        var detailBody = await detail.Content.ReadFromJsonAsync<ContentTypeDetailResponse>();

        var updateRequest = new UpdateContentTypeRequest(
            detailBody!.RowVersion, detailBody.ListTemplate, detailBody.DetailTemplate, detailBody.SortMode, detailBody.SortOrder,
            detailBody.SupportsHierarchy, detailBody.SupportsCategories, detailBody.SupportsTags, detailBody.SupportsDetailImage,
            detailBody.SupportsGallery, detailBody.SupportsVideos, detailBody.SupportsAttachments, detailBody.SupportsEvent,
            detailBody.SupportsBlockLayout, detailBody.SupportsForm, detailBody.SupportsRelatedContent, detailBody.HasDetailPage,
            detailBody.HasListingPage, detailBody.IsSearchable, detailBody.RequiresReview, SchemaKind: "NotARealKind");

        var response = await _client.SendAsync(Authorized(HttpMethod.Put, $"/api/v1/admin/website/content-types/{typeId}", accessToken, updateRequest));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // --- Public detail: jsonLd ---

    [Fact]
    public async Task GetPublicContentById_NewsArticleType_JsonLdContainsBreadcrumbListAndNewsArticleWithoutAuthor()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var title = $"Haber-{Guid.NewGuid():N}";
        var item = await CreateContentItemAsync(accessToken, newsTypeId, title, body: "<p>Gövde</p>", summary: "Özet");
        await PublishAsync(accessToken, item.Id);

        var response = await _client.GetAsync($"/api/v1/public/contents/{item.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var root = await ParseAsync(response);

        var jsonLd = root.GetProperty("jsonLd");
        Assert.Equal(JsonValueKind.Array, jsonLd.ValueKind);
        var entries = jsonLd.EnumerateArray().ToList();
        Assert.Equal(2, entries.Count);

        var breadcrumbList = entries[0];
        Assert.Equal("https://schema.org", breadcrumbList.GetProperty("@context").GetString());
        Assert.Equal("BreadcrumbList", breadcrumbList.GetProperty("@type").GetString());
        Assert.True(breadcrumbList.GetProperty("itemListElement").GetArrayLength() >= 2);

        var article = entries[1];
        Assert.Equal("NewsArticle", article.GetProperty("@type").GetString());
        Assert.Equal(title, article.GetProperty("headline").GetString());
        Assert.False(article.TryGetProperty("author", out _));
        Assert.True(article.TryGetProperty("mainEntityOfPage", out var mainEntityOfPage));
        Assert.Equal("WebPage", mainEntityOfPage.GetProperty("@type").GetString());
    }

    [Fact]
    public async Task GetPublicContentById_ArticleType_UsesArticleSchemaType()
    {
        var accessToken = await LoginAsAdminAsync();
        var projectTypeId = await GetContentTypeIdByKeyAsync(accessToken, "project");
        var item = await CreateContentItemAsync(accessToken, projectTypeId, $"Proje-{Guid.NewGuid():N}");
        await PublishAsync(accessToken, item.Id);

        var response = await _client.GetAsync($"/api/v1/public/contents/{item.Id}");
        var root = await ParseAsync(response);

        var entries = root.GetProperty("jsonLd").EnumerateArray().ToList();
        Assert.Contains(entries, e => e.GetProperty("@type").GetString() == "Article");
    }

    [Fact]
    public async Task GetPublicContentById_TypeWithNoneSchemaKindAndNoEvent_JsonLdContainsOnlyBreadcrumbList()
    {
        var accessToken = await LoginAsAdminAsync();
        var pageTypeId = await GetContentTypeIdByKeyAsync(accessToken, "page");
        var item = await CreateContentItemAsync(accessToken, pageTypeId, $"Sayfa-{Guid.NewGuid():N}");
        await PublishAsync(accessToken, item.Id);

        var response = await _client.GetAsync($"/api/v1/public/contents/{item.Id}");
        var root = await ParseAsync(response);

        var entries = root.GetProperty("jsonLd").EnumerateArray().ToList();
        var single = Assert.Single(entries);
        Assert.Equal("BreadcrumbList", single.GetProperty("@type").GetString());
    }

    [Fact]
    public async Task GetPublicContentById_EventType_JsonLdContainsEventWithoutOnlineLinkAnywhere()
    {
        var accessToken = await LoginAsAdminAsync();
        var eventTypeId = await GetContentTypeIdByKeyAsync(accessToken, "event");
        var item = await CreateContentItemAsync(accessToken, eventTypeId, $"Etkinlik-{Guid.NewGuid():N}");

        var scheduleRequest = new UpsertEventScheduleRequest(
            null, DateTime.UtcNow.AddDays(10), DateTime.UtcNow.AddDays(10).AddHours(2), "Hybrid", "https://meet.example.org/x", 10, true,
            null, null, null, null, true, true,
            [new UpsertEventScheduleTranslationInput("tr", "Salon A", "Adres No:1", "Ücretsiz", "Eğitmen", "<p>Program</p>", "Not")]);
        var scheduleResponse = await _client.SendAsync(
            Authorized(HttpMethod.Put, $"/api/v1/admin/website/contents/{item.Id}/event", accessToken, scheduleRequest));
        Assert.Equal(HttpStatusCode.OK, scheduleResponse.StatusCode);

        await PublishAsync(accessToken, item.Id);

        var response = await _client.GetAsync($"/api/v1/public/contents/{item.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var raw = await response.Content.ReadAsStringAsync();

        // Regression guard for ADR-024 §11.3/§15: OnlineLink must not appear anywhere in the response,
        // including inside jsonLd, even though the schedule above has one.
        Assert.DoesNotContain("meet.example.org", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onlineLink", raw, StringComparison.OrdinalIgnoreCase);

        var root = JsonDocument.Parse(raw).RootElement;
        var entries = root.GetProperty("jsonLd").EnumerateArray().ToList();
        var eventJsonLd = Assert.Single(entries, e => e.GetProperty("@type").GetString() == "Event");
        Assert.Equal("https://schema.org/EventScheduled", eventJsonLd.GetProperty("eventStatus").GetString());
        Assert.Equal("https://schema.org/MixedEventAttendanceMode", eventJsonLd.GetProperty("eventAttendanceMode").GetString());
        var location = eventJsonLd.GetProperty("location");
        Assert.Equal("Place", location.GetProperty("@type").GetString());
        Assert.Equal("Salon A", location.GetProperty("name").GetString());
    }

    // --- Public preview: never includes jsonLd ---

    [Fact]
    public async Task ContentPreviewResponse_HasNoJsonLdProperty()
    {
        // Compile-time guarantee, not a runtime HTTP check: ContentPreviewResponse (Görev 6's "Önizleme
        // yanıtında JSON-LD dönmez") carries no JsonLd member at all, unlike PublicContentDetailResponse.
        var properties = typeof(ContentPreviewResponse).GetProperties().Select(p => p.Name);
        Assert.DoesNotContain("JsonLd", properties);
    }

    // --- Public list: FAQPage jsonLd ---

    [Fact]
    public async Task GetPublicContents_FaqType_JsonLdContainsFaqPageWithPlainTextAnswers()
    {
        var accessToken = await LoginAsAdminAsync();
        var faqTypeId = await GetContentTypeIdByKeyAsync(accessToken, "faq");
        var question = $"Soru-{Guid.NewGuid():N}?";
        var item = await CreateContentItemAsync(accessToken, faqTypeId, question, body: "<p>Cevap <strong>metni</strong>.</p>");
        await PublishAsync(accessToken, item.Id);

        var response = await _client.GetAsync("/api/v1/public/contents?type=faq&pageSize=100");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var root = await ParseAsync(response);

        Assert.True(root.TryGetProperty("jsonLd", out var jsonLd));
        Assert.Equal("FAQPage", jsonLd.GetProperty("@type").GetString());
        var mainEntity = jsonLd.GetProperty("mainEntity").EnumerateArray().ToList();
        var ours = Assert.Single(mainEntity, q => q.GetProperty("name").GetString() == question);
        Assert.Equal("Cevap metni .", ours.GetProperty("acceptedAnswer").GetProperty("text").GetString());
    }

    [Fact]
    public async Task GetPublicContents_NewsType_JsonLdIsNull()
    {
        var response = await _client.GetAsync("/api/v1/public/contents?type=news");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var root = await ParseAsync(response);

        Assert.True(root.TryGetProperty("jsonLd", out var jsonLd));
        Assert.Equal(JsonValueKind.Null, jsonLd.ValueKind);
    }

    // --- Public site: organization ---

    [Fact]
    public async Task GetPublicSite_AfterConfiguringContactAndLogo_OrganizationIncludesThem()
    {
        var accessToken = await LoginAsAdminAsync();

        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(data.ToArray());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "logo.png");
        var uploadRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/website/media") { Content = content };
        uploadRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var uploadResponse = await _client.SendAsync(uploadRequest);
        var uploaded = await uploadResponse.Content.ReadFromJsonAsync<JsonElement>();
        var logoId = uploaded.GetProperty("id").GetGuid();

        var settingsResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/settings", accessToken));
        var settings = await settingsResponse.Content.ReadFromJsonAsync<JsonElement>();
        var rowVersion = settings.GetProperty("rowVersion").Deserialize<byte[]>();

        var identityResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, "/api/v1/admin/website/settings/identity", accessToken,
            new UpdateSiteSettingsIdentityRequest(rowVersion, logoId, null, null, null, [])));
        Assert.Equal(HttpStatusCode.NoContent, identityResponse.StatusCode);

        var contactRowVersion = (await (await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/admin/website/settings", accessToken))).Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("rowVersion").Deserialize<byte[]>();
        var contactResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, "/api/v1/admin/website/settings/contact", accessToken,
            new UpdateSiteSettingsContactRequest(
                contactRowVersion, "Örnek Mah. No:1, İstanbul", "+90 555 000 00 00", "info@example.org", null, null,
                [new UpdateSiteSettingsSocialLinkInput("Instagram", "https://instagram.com/x", 1)])));
        Assert.Equal(HttpStatusCode.NoContent, contactResponse.StatusCode);

        var response = await _client.GetAsync("/api/v1/public/site");
        var root = await ParseAsync(response);

        var organization = root.GetProperty("organization");
        Assert.True(organization.TryGetProperty("logo", out _));
        Assert.True(organization.TryGetProperty("contactPoint", out var contactPoint));
        Assert.Equal("+90 555 000 00 00", contactPoint.GetProperty("telephone").GetString());
        Assert.Equal("info@example.org", contactPoint.GetProperty("email").GetString());
        Assert.Equal("Örnek Mah. No:1, İstanbul", organization.GetProperty("address").GetString());
        var sameAs = organization.GetProperty("sameAs").EnumerateArray().Select(e => e.GetString()).ToList();
        Assert.Contains("https://instagram.com/x", sameAs);
    }
}
