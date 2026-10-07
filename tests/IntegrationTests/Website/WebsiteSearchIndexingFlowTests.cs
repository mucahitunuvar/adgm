using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.ArchiveContentItem;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.CreateContentType;
using GenclikMerkezi.Modules.Website.Features.DeactivateContentType;
using GenclikMerkezi.Modules.Website.Features.DeleteContentItem;
using GenclikMerkezi.Modules.Website.Features.GetContentItemById;
using GenclikMerkezi.Modules.Website.Features.GetContentTypeById;
using GenclikMerkezi.Modules.Website.Features.GetContentTypes;
using GenclikMerkezi.Modules.Website.Features.GetSiteLanguages;
using GenclikMerkezi.Modules.Website.Features.PublishContentItem;
using GenclikMerkezi.Modules.Website.Features.ScheduleContentItem;
using GenclikMerkezi.Modules.Website.Features.UnpublishContentItem;
using GenclikMerkezi.Modules.Website.Features.UpdateContentItemTranslation;
using GenclikMerkezi.Modules.Website.Features.UpdateContentTypeTranslation;
using GenclikMerkezi.Modules.Website.Infrastructure;
using GenclikMerkezi.Modules.Website.Infrastructure.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §10 (Faz 5 Görev 2). No feature/HTTP endpoint reads SearchDocument yet (Görev 3 adds the
// public search endpoint), so every assertion here resolves ISearchDocumentRepository directly from a
// DI scope - the same no-HTTP-endpoint pattern SearchDocumentPersistenceFlowTests (Görev 1) uses.
public class WebsiteSearchIndexingFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly CreateContentItemSeoInput EmptyItemSeo = new(null, null, null, null, null, null, null, false);
    private static readonly CreateContentTypeSeoInput EmptyTypeSeo = new(null, null, null, null, null, null, null, false);
    private static readonly UpdateContentTypeTranslationSeoInput EmptyTypeTranslationSeo = new(null, null, null, null, null, null, null, false);

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public WebsiteSearchIndexingFlowTests(CustomWebApplicationFactory factory)
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

    // A fresh, isolated searchable content type - HasDetailPage/IsSearchable controllable per test,
    // hierarchy always on (depth-3 cases need it, and it costs nothing for flat-item tests).
    private async Task<Guid> CreateContentTypeAsync(
        string accessToken, string keySuffix, bool isSearchable = true, bool hasDetailPage = true)
    {
        var key = $"srch-{keySuffix}-{Guid.NewGuid():N}"[..30];
        var request = new CreateContentTypeRequest(
            key, "list", "page", "Manual", 1,
            SupportsHierarchy: true, SupportsCategories: false, SupportsTags: false, SupportsDetailImage: false,
            SupportsGallery: false, SupportsVideos: false, SupportsAttachments: false, SupportsEvent: false,
            SupportsBlockLayout: false, SupportsForm: false, SupportsRelatedContent: false, HasDetailPage: hasDetailPage,
            HasListingPage: false, IsSearchable: isSearchable, RequiresReview: false,
            DefaultLanguageName: "Test Arama Türü", DefaultLanguageRoutePrefix: key, Seo: EmptyTypeSeo);
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/content-types", accessToken, request));
        var created = await response.Content.ReadFromJsonAsync<CreateContentTypeResponse>();
        return created!.Id;
    }

    private async Task<byte[]> GetContentTypeRowVersionAsync(string accessToken, Guid contentTypeId)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/content-types/{contentTypeId}", accessToken));
        var detail = await response.Content.ReadFromJsonAsync<ContentTypeDetailResponse>();
        return detail!.RowVersion;
    }

    private async Task AddContentTypeLanguageAsync(string accessToken, Guid contentTypeId, string languageCode, string routePrefix)
    {
        var rowVersion = await GetContentTypeRowVersionAsync(accessToken, contentTypeId);
        var request = new UpdateContentTypeTranslationRequest(rowVersion, "Test Arama Türü", routePrefix, EmptyTypeTranslationSeo);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/content-types/{contentTypeId}/translations/{languageCode}", accessToken, request));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // "en" is seeded from day one, inactive (GenclikMerkeziContentTypeSeed's remarks) - so the test
    // activates the existing seeded language rather than creating a new one (which would 409).
    private async Task<Guid> ActivateEnglishLanguageAsync(string accessToken)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/languages", accessToken));
        var languages = await response.Content.ReadFromJsonAsync<GetSiteLanguagesResponse>();
        var english = languages!.Items.Single(l => l.Code == "en");

        if (!english.IsActive)
        {
            var activateResponse = await _client.SendAsync(Authorized(
                HttpMethod.Post, $"/api/v1/admin/website/languages/{english.Id}/activate", accessToken));
            Assert.Equal(HttpStatusCode.NoContent, activateResponse.StatusCode);
        }

        return english.Id;
    }

    private async Task DeactivateSiteLanguageAsync(string accessToken, Guid languageId)
    {
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/languages/{languageId}/deactivate", accessToken));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<CreateContentItemResponse> CreateContentItemAsync(
        string accessToken, Guid contentTypeId, string title, Guid? parentId = null)
    {
        var request = new CreateContentItemRequest(
            contentTypeId, parentId, 1, false, null, null, title, $"{title}-{Guid.NewGuid():N}", null, null, EmptyItemSeo);
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/contents", accessToken, request));
        return (await response.Content.ReadFromJsonAsync<CreateContentItemResponse>())!;
    }

    private async Task<ContentItemDetailResponse> GetContentItemAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{id}", accessToken));
        return (await response.Content.ReadFromJsonAsync<ContentItemDetailResponse>())!;
    }

    private async Task AddContentItemTranslationAsync(
        string accessToken, Guid id, string languageCode, string title)
    {
        var item = await GetContentItemAsync(accessToken, id);
        var request = new UpdateContentItemTranslationRequest(
            item.RowVersion, title, $"{title}-{Guid.NewGuid():N}", $"{title} özeti", $"<p>{title} gövde</p>",
            new UpdateContentItemTranslationSeoInput(null, null, null, null, null, null, null, false));
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{id}/translations/{languageCode}", accessToken, request));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<HttpResponseMessage> PublishAsync(
        string accessToken, Guid id, DateTime? publishAtUtc = null, DateTime? unpublishAtUtc = null)
    {
        var item = await GetContentItemAsync(accessToken, id);
        return await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{id}/publish", accessToken,
            new PublishContentItemRequest(item.RowVersion, publishAtUtc, unpublishAtUtc)));
    }

    private async Task<HttpResponseMessage> UnpublishAsync(string accessToken, Guid id)
    {
        var item = await GetContentItemAsync(accessToken, id);
        return await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{id}/unpublish", accessToken,
            new UnpublishContentItemRequest(item.RowVersion)));
    }

    private async Task<HttpResponseMessage> ArchiveAsync(string accessToken, Guid id)
    {
        var item = await GetContentItemAsync(accessToken, id);
        return await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{id}/archive", accessToken,
            new ArchiveContentItemRequest(item.RowVersion)));
    }

    private async Task<HttpResponseMessage> MoveToTrashAsync(string accessToken, Guid id)
    {
        var item = await GetContentItemAsync(accessToken, id);
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/admin/website/contents/{id}")
        {
            Content = JsonContent.Create(new DeleteContentItemRequest(item.RowVersion)),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return await _client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> ScheduleAsync(
        string accessToken, Guid id, DateTime? publishAtUtc, DateTime? unpublishAtUtc)
    {
        var item = await GetContentItemAsync(accessToken, id);
        return await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{id}/schedule", accessToken,
            new ScheduleContentItemRequest(item.RowVersion, publishAtUtc, unpublishAtUtc)));
    }

    private async Task<SearchDocument?> GetSearchDocumentAsync(Guid contentItemId, string languageCode)
    {
        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchDocumentRepository>();
        return await repository.GetAsync("website", contentItemId.ToString(), LanguageCode.Create(languageCode).Value);
    }

    private async Task RunReconcileJobAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<ReconcileWebsiteSearchIndexJob>();
        await job.ExecuteAsync(CancellationToken.None);
    }

    // --- Visibility table (ADR-024 §4.4 / §10) ---

    [Fact]
    public async Task Draft_IsNotIndexed()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateContentTypeAsync(accessToken, "draft");
        var item = await CreateContentItemAsync(accessToken, typeId, "Taslak İçerik");

        Assert.Null(await GetSearchDocumentAsync(item.Id, "tr"));
    }

    [Fact]
    public async Task PublishedWithFuturePublishAtUtc_IsNotIndexedYet()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateContentTypeAsync(accessToken, "scheduled");
        var item = await CreateContentItemAsync(accessToken, typeId, "Zamanlanmış İçerik");

        var response = await PublishAsync(accessToken, item.Id, DateTime.UtcNow.AddDays(1));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null(await GetSearchDocumentAsync(item.Id, "tr"));
    }

    [Fact]
    public async Task PublishedWithPastUnpublishAtUtc_IsExpiredAndNotIndexed()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateContentTypeAsync(accessToken, "expired");
        var item = await CreateContentItemAsync(accessToken, typeId, "Süresi Dolmuş İçerik");

        // ScheduleContentItemCommand only accepts this combination post-publish (Schedule requires an
        // already-Published item) - publish with a short future window first, then immediately
        // reschedule its end into the past via direct persistence (the same test-only bypass
        // EventRegistrationFlowTests uses for fields no admin endpoint exposes).
        await PublishAsync(accessToken, item.Id, null, DateTime.UtcNow.AddMinutes(5));

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<WebsiteDbContext>();
            var contentItem = await dbContext.ContentItems.FirstAsync(c => c.Id == item.Id);
            dbContext.Entry(contentItem).Property(nameof(ContentItem.UnpublishAtUtc)).CurrentValue = DateTime.UtcNow.AddMinutes(-1);
            await dbContext.SaveChangesAsync();
        }

        await RunReconcileJobAsync();

        Assert.Null(await GetSearchDocumentAsync(item.Id, "tr"));
    }

    [Fact]
    public async Task Archived_IsNotIndexed()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateContentTypeAsync(accessToken, "archived");
        var item = await CreateContentItemAsync(accessToken, typeId, "Arşivlenecek İçerik");
        await PublishAsync(accessToken, item.Id);
        Assert.NotNull(await GetSearchDocumentAsync(item.Id, "tr"));

        await UnpublishAsync(accessToken, item.Id);
        var archiveResponse = await ArchiveAsync(accessToken, item.Id);

        Assert.Equal(HttpStatusCode.NoContent, archiveResponse.StatusCode);
        Assert.Null(await GetSearchDocumentAsync(item.Id, "tr"));
    }

    [Fact]
    public async Task MovedToTrash_IsNotIndexed()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateContentTypeAsync(accessToken, "trash");
        var item = await CreateContentItemAsync(accessToken, typeId, "Çöpe Taşınacak İçerik");
        await PublishAsync(accessToken, item.Id);
        await UnpublishAsync(accessToken, item.Id);

        var trashResponse = await MoveToTrashAsync(accessToken, item.Id);

        Assert.Equal(HttpStatusCode.NoContent, trashResponse.StatusCode);
        Assert.Null(await GetSearchDocumentAsync(item.Id, "tr"));
    }

    [Fact]
    public async Task ContentTypeNotSearchable_NeverIndexed()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateContentTypeAsync(accessToken, "notsearchable", isSearchable: false);
        var item = await CreateContentItemAsync(accessToken, typeId, "Aranamaz Tür İçeriği");

        var publishResponse = await PublishAsync(accessToken, item.Id);

        Assert.Equal(HttpStatusCode.NoContent, publishResponse.StatusCode);
        Assert.Null(await GetSearchDocumentAsync(item.Id, "tr"));
    }

    [Fact]
    public async Task ContentTypeWithoutDetailPage_NeverIndexed()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateContentTypeAsync(accessToken, "nodetail", hasDetailPage: false);
        var item = await CreateContentItemAsync(accessToken, typeId, "Detay Sayfası Olmayan İçerik");

        await PublishAsync(accessToken, item.Id);

        Assert.Null(await GetSearchDocumentAsync(item.Id, "tr"));
    }

    [Fact]
    public async Task ContentTypeDeactivated_RemovesAlreadyIndexedItems()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateContentTypeAsync(accessToken, "typeoff");
        var item = await CreateContentItemAsync(accessToken, typeId, "Türü Pasifleşecek İçerik");
        await PublishAsync(accessToken, item.Id);
        Assert.NotNull(await GetSearchDocumentAsync(item.Id, "tr"));

        var typeRowVersion = await GetContentTypeRowVersionAsync(accessToken, typeId);
        var deactivateResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/content-types/{typeId}/deactivate", accessToken,
            new DeactivateContentTypeRequest(typeRowVersion)));

        Assert.Equal(HttpStatusCode.NoContent, deactivateResponse.StatusCode);
        Assert.Null(await GetSearchDocumentAsync(item.Id, "tr"));
    }

    [Fact]
    public async Task LanguageDeactivated_RemovesOnlyThatLanguagesDocument()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateContentTypeAsync(accessToken, "lang");
        await AddContentTypeLanguageAsync(accessToken, typeId, "en", $"en-prefix-{Guid.NewGuid():N}"[..20]);
        var languageId = await ActivateEnglishLanguageAsync(accessToken);

        var item = await CreateContentItemAsync(accessToken, typeId, "Çok Dilli İçerik");
        await AddContentItemTranslationAsync(accessToken, item.Id, "en", "Multilingual Content");
        await PublishAsync(accessToken, item.Id);

        Assert.NotNull(await GetSearchDocumentAsync(item.Id, "tr"));
        Assert.NotNull(await GetSearchDocumentAsync(item.Id, "en"));

        await DeactivateSiteLanguageAsync(accessToken, languageId);

        Assert.NotNull(await GetSearchDocumentAsync(item.Id, "tr"));
        Assert.Null(await GetSearchDocumentAsync(item.Id, "en"));
    }

    [Fact]
    public async Task InvisibleAncestor_ChildIsNotIndexed_EvenThoughItsOwnStatusIsPublished()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateContentTypeAsync(accessToken, "ancestor");
        var parent = await CreateContentItemAsync(accessToken, typeId, "Görünmez Ebeveyn");
        // Parent published but scheduled far in the future - Published status, not yet visible.
        await PublishAsync(accessToken, parent.Id, DateTime.UtcNow.AddDays(7));
        var child = await CreateContentItemAsync(accessToken, typeId, "Görünür Olması Gereken Çocuk", parent.Id);

        var childPublishResponse = await PublishAsync(accessToken, child.Id);

        Assert.Equal(HttpStatusCode.NoContent, childPublishResponse.StatusCode);
        Assert.Null(await GetSearchDocumentAsync(child.Id, "tr"));
    }

    // --- Hierarchical visibility cascade ---

    [Fact]
    public async Task AncestorBecomesInvisible_RemovesEveryDescendantsDocumentAcrossTheWholeSubtree()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateContentTypeAsync(accessToken, "cascade-drop");
        var root = await CreateContentItemAsync(accessToken, typeId, "Kök");
        await PublishAsync(accessToken, root.Id);
        var middle = await CreateContentItemAsync(accessToken, typeId, "Orta", root.Id);
        await PublishAsync(accessToken, middle.Id);
        var leaf = await CreateContentItemAsync(accessToken, typeId, "Yaprak", middle.Id);
        await PublishAsync(accessToken, leaf.Id);

        Assert.NotNull(await GetSearchDocumentAsync(root.Id, "tr"));
        Assert.NotNull(await GetSearchDocumentAsync(middle.Id, "tr"));
        Assert.NotNull(await GetSearchDocumentAsync(leaf.Id, "tr"));

        // Root becomes invisible (rescheduled into the future) without touching middle/leaf's own
        // Published status at all - both descendants' documents must still be removed, proving the
        // cascade walks the whole ancestor chain (ADR-024 §4.4), not just the immediate parent.
        var rescheduleResponse = await ScheduleAsync(accessToken, root.Id, DateTime.UtcNow.AddDays(7), null);

        Assert.Equal(HttpStatusCode.NoContent, rescheduleResponse.StatusCode);
        Assert.Null(await GetSearchDocumentAsync(root.Id, "tr"));
        Assert.Null(await GetSearchDocumentAsync(middle.Id, "tr"));
        Assert.Null(await GetSearchDocumentAsync(leaf.Id, "tr"));
    }

    // --- Reconciliation job (scheduled visibility changes raise no mutation event) ---

    [Fact]
    public async Task ReconcileJob_SchedulePublishCrossingNow_AddsTheDocument()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateContentTypeAsync(accessToken, "reconcile-add");
        var item = await CreateContentItemAsync(accessToken, typeId, "Yakında Yayınlanacak İçerik");
        await PublishAsync(accessToken, item.Id, DateTime.UtcNow.AddSeconds(3));
        Assert.Null(await GetSearchDocumentAsync(item.Id, "tr"));

        await Task.Delay(TimeSpan.FromSeconds(4));
        await RunReconcileJobAsync();

        Assert.NotNull(await GetSearchDocumentAsync(item.Id, "tr"));
    }

    [Fact]
    public async Task ReconcileJob_ScheduledUnpublishCrossingNow_RemovesTheDocument()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateContentTypeAsync(accessToken, "reconcile-remove");
        var item = await CreateContentItemAsync(accessToken, typeId, "Yakında Yayından Kalkacak İçerik");
        await PublishAsync(accessToken, item.Id, null, DateTime.UtcNow.AddSeconds(3));
        Assert.NotNull(await GetSearchDocumentAsync(item.Id, "tr"));

        await Task.Delay(TimeSpan.FromSeconds(4));
        await RunReconcileJobAsync();

        Assert.Null(await GetSearchDocumentAsync(item.Id, "tr"));
    }

    // --- NoIndex / sitemap flag ---

    [Fact]
    public async Task NoIndexContent_IsStillSearchable_ButExcludedFromSitemap()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateContentTypeAsync(accessToken, "noindex");
        var item = await CreateContentItemAsync(accessToken, typeId, "NoIndex İçerik");

        var current = await GetContentItemAsync(accessToken, item.Id);
        var request = new UpdateContentItemTranslationRequest(
            current.RowVersion, "NoIndex İçerik", current.Translations[0].Slug, "Özet", "<p>Gövde</p>",
            new UpdateContentItemTranslationSeoInput(null, null, null, null, null, null, null, true));
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{item.Id}/translations/tr", accessToken, request));
        await PublishAsync(accessToken, item.Id);

        var document = await GetSearchDocumentAsync(item.Id, "tr");

        Assert.NotNull(document);
        Assert.False(document!.IncludeInSitemap);
    }

    // --- Transaction atomicity ---

    [Fact]
    public async Task SaveChangesFailure_RollsBackBothTheContentItemAndTheSearchDocumentChange()
    {
        var accessToken = await LoginAsAdminAsync();
        var typeId = await CreateContentTypeAsync(accessToken, "atomic");
        var item = await CreateContentItemAsync(accessToken, typeId, "Atomiklik İçeriği");
        await PublishAsync(accessToken, item.Id);
        var originalDocument = await GetSearchDocumentAsync(item.Id, "tr");
        Assert.NotNull(originalDocument);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<WebsiteDbContext>();
            var contentItem = await dbContext.ContentItems.FirstAsync(c => c.Id == item.Id);
            contentItem.Unpublish(0, Guid.NewGuid(), DateTime.UtcNow);

            // A second document with the already-indexed natural key, in the SAME DbContext/transaction
            // as the Unpublish() above - forces SaveChangesAsync to throw on the unique index, so
            // neither pending change is committed.
            var conflicting = SearchDocument.Create(
                "website", item.Id.ToString(), LanguageCode.Create("tr").Value, "conflict", "Çakışan Başlık",
                "Özet", "/cakisan", "CAKISAN", DateTime.UtcNow, DateTime.UtcNow, true).Value;
            dbContext.SearchDocuments.Add(conflicting);

            await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
        }

        using (var verifyScope = _factory.Services.CreateScope())
        {
            var dbContext = verifyScope.ServiceProvider.GetRequiredService<WebsiteDbContext>();
            var contentItem = await dbContext.ContentItems.AsNoTracking().FirstAsync(c => c.Id == item.Id);
            Assert.Equal(ContentItemStatus.Published, contentItem.Status);
        }

        var documentAfterFailure = await GetSearchDocumentAsync(item.Id, "tr");
        Assert.NotNull(documentAfterFailure);
        Assert.Equal(originalDocument!.Title, documentAfterFailure!.Title);
    }
}
