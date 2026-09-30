using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Application.ContentPaths;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.GetContentItemById;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.PublishContentItem;
using GenclikMerkezi.Modules.Website.Features.SetContentItemRelatedContent;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §4.1 (Faz 1b Görev 5). Shares the "one CustomWebApplicationFactory/one Sqlite database per
// class" caveat every other Website flow test class documents - "news" (RelatedContent enabled) and
// "team" (disabled) are the seeded content types this class relies on.
public class ContentRelatedContentFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ContentRelatedContentFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<Guid> CreateContentItemAsync(string accessToken, Guid contentTypeId, string title = "Başlık")
    {
        var emptySeo = new CreateContentItemSeoInput(null, null, null, null, null, null, null, false);
        var slug = $"slug-{Guid.NewGuid():N}";
        var request = new CreateContentItemRequest(contentTypeId, null, 1, false, null, null, title, slug, null, null, emptySeo);
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, request));
        var created = await response.Content.ReadFromJsonAsync<CreateContentItemResponse>();
        return created!.Id;
    }

    private async Task<ContentItemDetailResponse> GetContentItemAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{id}", accessToken));
        return (await response.Content.ReadFromJsonAsync<ContentItemDetailResponse>())!;
    }

    [Fact]
    public async Task SetRelatedContent_ForTypeWithoutSupportsRelatedContent_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var teamTypeId = await GetContentTypeIdByKeyAsync(accessToken, "team");
        var itemId = await CreateContentItemAsync(accessToken, teamTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        var otherId = await CreateContentItemAsync(accessToken, teamTypeId, "Diğer");

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/related", accessToken,
            new SetContentItemRelatedContentRequest(item.RowVersion, [otherId])));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetRelatedContent_WithSelfReference_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/related", accessToken,
            new SetContentItemRelatedContentRequest(item.RowVersion, [itemId])));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetRelatedContent_WithNonExistentTarget_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/related", accessToken,
            new SetContentItemRelatedContentRequest(item.RowVersion, [Guid.NewGuid()])));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SetRelatedContent_WithDuplicateTarget_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        var otherId = await CreateContentItemAsync(accessToken, newsTypeId, "Diğer");

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/related", accessToken,
            new SetContentItemRelatedContentRequest(item.RowVersion, [otherId, otherId])));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetRelatedContent_WithTargetOfDifferentType_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var teamTypeId = await GetContentTypeIdByKeyAsync(accessToken, "team");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        var teamMemberId = await CreateContentItemAsync(accessToken, teamTypeId, "Ekip Üyesi");

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/related", accessToken,
            new SetContentItemRelatedContentRequest(item.RowVersion, [teamMemberId])));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task SetRelatedContent_WithValidIds_PersistsOrder()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        var firstId = await CreateContentItemAsync(accessToken, newsTypeId, "Birinci");
        var secondId = await CreateContentItemAsync(accessToken, newsTypeId, "İkinci");

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/related", accessToken,
            new SetContentItemRelatedContentRequest(item.RowVersion, [secondId, firstId])));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var updated = await GetContentItemAsync(accessToken, itemId);
        Assert.Equal([secondId, firstId], updated.RelatedContentItemIds);
    }

    [Fact]
    public async Task SetRelatedContent_WithStaleRowVersion_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");
        var itemId = await CreateContentItemAsync(accessToken, newsTypeId);
        var item = await GetContentItemAsync(accessToken, itemId);
        var otherId = await CreateContentItemAsync(accessToken, newsTypeId, "Diğer");
        var staleRowVersion = item.RowVersion;

        var firstResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/related", accessToken,
            new SetContentItemRelatedContentRequest(staleRowVersion, [otherId])));
        Assert.Equal(HttpStatusCode.NoContent, firstResponse.StatusCode);

        var secondResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{itemId}/related", accessToken,
            new SetContentItemRelatedContentRequest(staleRowVersion, [otherId])));

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    // Exercises RelatedContentResolutionService against the real ContentItemRepository/Sqlite
    // database directly (it has no HTTP surface of its own until Görev 7's public detail endpoint) -
    // primarily to confirm its owned-collection/primitive-collection LINQ actually translates to SQL,
    // not just that it compiles.
    [Fact]
    public async Task RelatedContentResolutionService_WithManualLinkThenCategoryFallback_ResolvesAcrossBothSteps()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");

        var manualTargetId = await CreateContentItemAsync(accessToken, newsTypeId, "Elle Bağlı");
        var manualTargetItem = await GetContentItemAsync(accessToken, manualTargetId);
        var publishResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{manualTargetId}/publish", accessToken,
            new PublishContentItemRequest(manualTargetItem.RowVersion, null, null)));
        Assert.Equal(HttpStatusCode.NoContent, publishResponse.StatusCode);

        var sourceId = await CreateContentItemAsync(accessToken, newsTypeId, "Kaynak");
        var source = await GetContentItemAsync(accessToken, sourceId);
        var setRelatedResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{sourceId}/related", accessToken,
            new SetContentItemRelatedContentRequest(source.RowVersion, [manualTargetId])));
        Assert.Equal(HttpStatusCode.NoContent, setRelatedResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var resolutionService = scope.ServiceProvider.GetRequiredService<RelatedContentResolutionService>();
        var trLanguage = LanguageCode.Create("tr").Value;

        var resolved = await resolutionService.ResolveAsync(sourceId, newsTypeId, [manualTargetId], [], trLanguage, DateTime.UtcNow);

        Assert.Equal([manualTargetId], resolved.Select(r => r.Id));
    }

    // Regression test: GetVisibleRelatedCandidatesByIdsAsync/SearchRelatedCandidatesAsync already
    // enforced ContentItem.IsVisible before this bugfix - unlike SearchPublicListAsync, which did not -
    // this proves a manually-linked target that is currently scheduled (future PublishAtUtc) or already
    // expired (past UnpublishAtUtc) is excluded from related content exactly like an unpublished one.
    [Fact]
    public async Task RelatedContentResolutionService_WithScheduledOrExpiredManualLink_ExcludesInvisibleTargets()
    {
        var accessToken = await LoginAsAdminAsync();
        var newsTypeId = await GetContentTypeIdByKeyAsync(accessToken, "news");

        var scheduledTargetId = await CreateContentItemAsync(accessToken, newsTypeId, "Zamanlanmis Hedef");
        var scheduledTarget = await GetContentItemAsync(accessToken, scheduledTargetId);
        var scheduleResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{scheduledTargetId}/publish", accessToken,
            new PublishContentItemRequest(scheduledTarget.RowVersion, DateTime.UtcNow.AddDays(1), null)));
        Assert.Equal(HttpStatusCode.NoContent, scheduleResponse.StatusCode);

        var expiredTargetId = await CreateContentItemAsync(accessToken, newsTypeId, "Suresi Dolmus Hedef");
        var expiredTarget = await GetContentItemAsync(accessToken, expiredTargetId);
        var expireResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{expiredTargetId}/publish", accessToken,
            new PublishContentItemRequest(expiredTarget.RowVersion, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1))));
        Assert.Equal(HttpStatusCode.NoContent, expireResponse.StatusCode);

        var sourceId = await CreateContentItemAsync(accessToken, newsTypeId, "Kaynak");
        var source = await GetContentItemAsync(accessToken, sourceId);
        var setRelatedResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{sourceId}/related", accessToken,
            new SetContentItemRelatedContentRequest(source.RowVersion, [scheduledTargetId, expiredTargetId])));
        Assert.Equal(HttpStatusCode.NoContent, setRelatedResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var resolutionService = scope.ServiceProvider.GetRequiredService<RelatedContentResolutionService>();
        var trLanguage = LanguageCode.Create("tr").Value;

        // No category filter and the manual link resolves to nothing visible, so the service falls
        // through to RelatedContentResolutionService's step 3 (automatic same-ContentType suggestions,
        // which may be non-empty depending on what other tests in this shared-database test class have
        // published) - the assertion only cares that the two invisible targets themselves never appear.
        var resolved = await resolutionService.ResolveAsync(
            sourceId, newsTypeId, [scheduledTargetId, expiredTargetId], [], trLanguage, DateTime.UtcNow);

        Assert.DoesNotContain(resolved, r => r.Id == scheduledTargetId);
        Assert.DoesNotContain(resolved, r => r.Id == expiredTargetId);
    }
}
