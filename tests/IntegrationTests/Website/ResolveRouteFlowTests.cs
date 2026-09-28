using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.CreateContentType;
using GenclikMerkezi.Modules.Website.Features.CreateRedirect;
using GenclikMerkezi.Modules.Website.Features.DeactivateContentType;
using GenclikMerkezi.Modules.Website.Features.GetContentItemById;
using GenclikMerkezi.Modules.Website.Features.GetContentTypeById;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.GetNotFoundPaths;
using GenclikMerkezi.Modules.Website.Features.PublishContentItem;
using GenclikMerkezi.Modules.Website.Features.ResolveRoute;
using GenclikMerkezi.Modules.Website.Features.UpdateContentItemTranslation;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §15 (Faz 1a Görev 6). Shares the "one CustomWebApplicationFactory/one Sqlite database per
// class" caveat every other Website flow test class documents. "en" is seeded inactive (Görev 2's
// content type seed comment) and this class never activates it, so it is safe to reuse directly as
// the "inactive language prefix" fixture across every test here regardless of xUnit's execution
// order; any ACTIVE non-default language a test needs is instead created fresh with a unique code.
public class ResolveRouteFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ResolveRouteFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<ContentItemDetailResponse> GetContentItemAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{id}", accessToken));
        return (await response.Content.ReadFromJsonAsync<ContentItemDetailResponse>())!;
    }

    private async Task<(Guid Id, string FullPath)> CreateContentItemAsync(
        string accessToken, Guid contentTypeId, Guid? parentId, string slug, string title = "Başlık")
    {
        var emptySeo = new CreateContentItemSeoInput(null, null, null, null, null, null, null, false);
        var request = new CreateContentItemRequest(contentTypeId, parentId, 1, false, null, null, title, slug, null, null, emptySeo);
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, request));
        var created = await response.Content.ReadFromJsonAsync<CreateContentItemResponse>();
        return (created!.Id, created.FullPath);
    }

    private async Task PublishAsync(string accessToken, Guid id, DateTime? publishAtUtc = null, DateTime? unpublishAtUtc = null)
    {
        var rowVersion = (await GetContentItemAsync(accessToken, id)).RowVersion;
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{id}/publish", accessToken,
            new PublishContentItemRequest(rowVersion, publishAtUtc, unpublishAtUtc)));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<RouteResolutionResponse> ResolveAsync(string path)
    {
        var response = await _client.GetAsync($"/api/v1/public/routes/resolve?path={Uri.EscapeDataString(path)}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<RouteResolutionResponse>())!;
    }

    [Fact]
    public async Task Resolve_EmptyPath_ReturnsHomeInDefaultLanguage()
    {
        var result = await ResolveAsync("/");

        Assert.Equal("Home", result.Kind);
        Assert.Equal("tr", result.LanguageCode);
    }

    [Fact]
    public async Task Resolve_ExactPublishedContentItemFullPath_ReturnsDetail()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var slug = $"detay-{Guid.NewGuid():N}";
        var item = await CreateContentItemAsync(accessToken, newsTypeId, null, slug);
        await PublishAsync(accessToken, item.Id);

        var result = await ResolveAsync("/" + item.FullPath);

        Assert.Equal("Detail", result.Kind);
        Assert.Equal("tr", result.LanguageCode);
        Assert.Equal(item.Id, result.ContentItemId);
        Assert.Equal("news", result.ContentTypeKey);
        Assert.NotNull(result.DetailTemplate);
    }

    [Fact]
    public async Task Resolve_DraftContentItemFullPath_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var slug = $"taslak-{Guid.NewGuid():N}";
        var item = await CreateContentItemAsync(accessToken, newsTypeId, null, slug);

        var result = await ResolveAsync("/" + item.FullPath);

        Assert.Equal("NotFound", result.Kind);
    }

    [Fact]
    public async Task Resolve_ContentTypeRoutePrefixAsSingleSegment_ReturnsListing()
    {
        var result = await ResolveAsync("/haberler");

        Assert.Equal("Listing", result.Kind);
        Assert.Equal("tr", result.LanguageCode);
        Assert.Equal("news", result.ContentTypeKey);
        Assert.Equal("Haber", result.Name);
    }

    [Fact]
    public async Task Resolve_UnknownPath_ReturnsNotFound_AndRecordsNotFoundLog()
    {
        var accessToken = await LoginAsAdminAsync();
        var unknownPath = $"bilinmeyen-{Guid.NewGuid():N}";

        var result = await ResolveAsync("/" + unknownPath);

        Assert.Equal("NotFound", result.Kind);
        Assert.Equal("tr", result.LanguageCode);

        // RecordNotFoundPathCommand is sent with the raw "path" query value verbatim (leading slash
        // and all) - it is not re-normalized before being logged (ADR-024 §15 only requires the
        // resolution result itself to be canonical, not the NotFoundLog row).
        var notFoundPathsResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/not-found-paths", accessToken));
        var notFoundPaths = await notFoundPathsResponse.Content.ReadFromJsonAsync<PagedResult<NotFoundLogResponse>>();
        var logged = Assert.Single(notFoundPaths!.Items, i => i.Path == "/" + unknownPath);
        Assert.Equal(1, logged.HitCount);
    }

    [Fact]
    public async Task Resolve_SameUnknownPathTwice_BumpsHitCountInsteadOfDuplicating()
    {
        var accessToken = await LoginAsAdminAsync();
        var unknownPath = $"tekrar-{Guid.NewGuid():N}";

        await ResolveAsync("/" + unknownPath);
        await ResolveAsync("/" + unknownPath);

        var notFoundPathsResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/not-found-paths", accessToken));
        var notFoundPaths = await notFoundPathsResponse.Content.ReadFromJsonAsync<PagedResult<NotFoundLogResponse>>();
        var logged = Assert.Single(notFoundPaths!.Items, i => i.Path == "/" + unknownPath);
        Assert.Equal(2, logged.HitCount);
    }

    [Fact]
    public async Task Resolve_DefaultLanguagePrefix_RedirectsToPathWithoutPrefix()
    {
        var result = await ResolveAsync("/tr/haberler");

        Assert.Equal("Redirect", result.Kind);
        Assert.Equal("/haberler", result.Location);
        Assert.Equal(301, result.StatusCode);
    }

    [Fact]
    public async Task Resolve_InactiveLanguagePrefix_ReturnsNotFound()
    {
        var result = await ResolveAsync("/en/news");

        Assert.Equal("NotFound", result.Kind);
        Assert.Equal("tr", result.LanguageCode);
    }

    [Fact]
    public async Task Resolve_UppercaseAndTrailingSlash_RedirectsToNormalizedCanonicalPath()
    {
        var result = await ResolveAsync("/HABERLER/");

        Assert.Equal("Redirect", result.Kind);
        Assert.Equal("/haberler", result.Location);
        Assert.Equal(301, result.StatusCode);
    }

    [Fact]
    public async Task Resolve_ChildUnderScheduledParent_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var pageTypeId = await GetContentTypeIdByKeyAsync(accessToken, "page");
        var parentSlug = $"zamanlanmis-ebeveyn-{Guid.NewGuid():N}";
        var parent = await CreateContentItemAsync(accessToken, pageTypeId, null, parentSlug);
        await PublishAsync(accessToken, parent.Id, publishAtUtc: DateTime.UtcNow.AddDays(7));

        var childSlug = $"cocuk-{Guid.NewGuid():N}";
        var child = await CreateContentItemAsync(accessToken, pageTypeId, parent.Id, childSlug);
        await PublishAsync(accessToken, child.Id);

        var result = await ResolveAsync("/" + child.FullPath);

        Assert.Equal("NotFound", result.Kind);
    }

    [Fact]
    public async Task Resolve_DetailPathForContentTypeWithoutDetailPage_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var teamTypeId = await GetContentTypeIdByKeyAsync(accessToken, "team");
        var slug = $"uye-{Guid.NewGuid():N}";
        var item = await CreateContentItemAsync(accessToken, teamTypeId, null, slug);
        await PublishAsync(accessToken, item.Id);

        var result = await ResolveAsync("/" + item.FullPath);

        Assert.Equal("NotFound", result.Kind);
    }

    [Fact]
    public async Task Resolve_DetailPathForInactiveContentType_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = $"test-{Guid.NewGuid():N}"[..20];
        var emptySeo = new CreateContentTypeSeoInput(null, null, null, null, null, null, null, false);
        var createTypeRequest = new CreateContentTypeRequest(
            key, "list", "article", "Manual", 99,
            SupportsHierarchy: false, SupportsCategories: false, SupportsTags: false, SupportsDetailImage: false,
            SupportsGallery: false, SupportsVideos: false, SupportsAttachments: false, SupportsEvent: false,
            SupportsBlockLayout: false, SupportsForm: false, SupportsRelatedContent: false, HasDetailPage: true,
            HasListingPage: false, IsSearchable: true, RequiresReview: false,
            DefaultLanguageName: "Test Türü", DefaultLanguageRoutePrefix: key, Seo: emptySeo);
        var createTypeResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, "/api/v1/admin/website/content-types", accessToken, createTypeRequest));
        var createdType = await createTypeResponse.Content.ReadFromJsonAsync<CreateContentTypeResponse>();

        var slug = $"icerik-{Guid.NewGuid():N}";
        var item = await CreateContentItemAsync(accessToken, createdType!.Id, null, slug);
        await PublishAsync(accessToken, item.Id);

        var typeDetailResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/content-types/{createdType.Id}", accessToken));
        var typeRowVersion = (await typeDetailResponse.Content.ReadFromJsonAsync<ContentTypeDetailResponse>())!.RowVersion;
        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/content-types/{createdType.Id}/deactivate", accessToken,
            new DeactivateContentTypeRequest(typeRowVersion)));

        var result = await ResolveAsync("/" + item.FullPath);

        Assert.Equal("NotFound", result.Kind);
    }

    [Fact]
    public async Task Resolve_PathMovedByAutomaticRedirect_RedirectsToCurrentPath()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var originalSlug = $"eski-slug-{Guid.NewGuid():N}";
        var item = await CreateContentItemAsync(accessToken, newsTypeId, null, originalSlug);
        var originalFullPath = item.FullPath;

        var rowVersion = (await GetContentItemAsync(accessToken, item.Id)).RowVersion;
        var newSlug = $"yeni-slug-{Guid.NewGuid():N}";
        var emptyTranslationSeo = new UpdateContentItemTranslationSeoInput(null, null, null, null, null, null, null, false);
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{item.Id}/translations/tr", accessToken,
            new UpdateContentItemTranslationRequest(rowVersion, "Başlık", newSlug, null, null, emptyTranslationSeo)));
        await PublishAsync(accessToken, item.Id);
        var newFullPath = (await GetContentItemAsync(accessToken, item.Id)).Translations.Single(t => t.LanguageCode == "tr").FullPath;

        var result = await ResolveAsync("/" + originalFullPath);

        Assert.Equal("Redirect", result.Kind);
        Assert.Equal("/" + newFullPath, result.Location);
        Assert.Equal(301, result.StatusCode);
    }

    [Fact]
    public async Task Resolve_ManualRedirectToInvisibleContentItem_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var slug = $"gorunmez-{Guid.NewGuid():N}";
        var target = await CreateContentItemAsync(accessToken, newsTypeId, null, slug);
        // target is intentionally left as Draft (never published) - not visible.

        var fromPath = $"eski-yonlendirme-{Guid.NewGuid():N}";
        await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/redirects", accessToken,
            new CreateRedirectRequest("tr", fromPath, "ContentItem", target.Id, null, "MovedPermanently")));

        var result = await ResolveAsync("/" + fromPath);

        Assert.Equal("NotFound", result.Kind);
    }

    [Fact]
    public async Task Resolve_ManualRedirectToStaticPath_ReturnsRedirect()
    {
        var accessToken = await LoginAsAdminAsync();
        var fromPath = $"eski-sayfa-{Guid.NewGuid():N}";
        await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/redirects", accessToken,
            new CreateRedirectRequest("tr", fromPath, "Path", null, "haberler", "Found")));

        var result = await ResolveAsync("/" + fromPath);

        Assert.Equal("Redirect", result.Kind);
        Assert.Equal("/haberler", result.Location);
        Assert.Equal(302, result.StatusCode);
    }

    [Fact]
    public async Task Resolve_ManualRedirectToAbsoluteUrl_ReturnsLocationVerbatim()
    {
        var accessToken = await LoginAsAdminAsync();
        var fromPath = $"disa-yonlendirme-{Guid.NewGuid():N}";
        await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/redirects", accessToken,
            new CreateRedirectRequest("tr", fromPath, "Path", null, "https://example.com/hedef", "MovedPermanently")));

        var result = await ResolveAsync("/" + fromPath);

        Assert.Equal("Redirect", result.Kind);
        Assert.Equal("https://example.com/hedef", result.Location);
    }
}
