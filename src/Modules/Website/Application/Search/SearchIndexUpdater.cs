using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.ContentPaths;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Search;

// ADR-024 §10 (Faz 5 Görev 2): ISearchIndexUpdater's implementation. Lives in Application, not
// Infrastructure - like ContentPathCascadeService/EventCancellationNotifier, it only orchestrates
// other repository ports (no EF Core/Hangfire dependency of its own); registered under both its
// concrete type and the interface (same DI shape as EventCancellationNotifier) so
// ContinueSearchIndexBatchJob (Infrastructure) can call the two extra batch-continuation methods that
// are not part of the public port.
public sealed class SearchIndexUpdater(
    IContentItemRepository contentItemRepository,
    IContentTypeRepository contentTypeRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ISearchDocumentRepository searchDocumentRepository,
    ContentPathCascadeService pathCascadeService,
    ISearchIndexBatchScheduler batchScheduler,
    TimeProvider timeProvider)
    : ISearchIndexUpdater
{
    public const string WebsiteSourceKey = "website";

    // "çok büyük ağaçta tek seferde en çok 500 içerik, kalanı Hangfire'a devredilir" (ADR-024 §10).
    public const int BatchSize = 500;

    // PagedRequest.MaxPageSize (100) is the shared cap every list query in the system respects
    // (ARCHITECTURE.md §9) - ContentType/language-wide batches page through it BatchSize/PageSize
    // times per call instead of raising that shared cap just for this one port.
    private const int PageSize = PagedRequest.MaxPageSize;

    public async Task ReindexAsync(ContentItem contentItem, CancellationToken cancellationToken = default)
    {
        var context = await LoadContextAsync(cancellationToken);
        await ReindexItemAsync(contentItem, context, cancellationToken);
    }

    public async Task ReindexWithDescendantsAsync(ContentItem contentItem, CancellationToken cancellationToken = default)
    {
        var descendantIds = new List<Guid>();
        await CollectDescendantIdsAsync(contentItem.Id, descendantIds, cancellationToken);

        var context = await LoadContextAsync(cancellationToken);
        await ReindexItemAsync(contentItem, context, cancellationToken);

        var batch = descendantIds.Take(BatchSize).ToList();
        foreach (var descendantId in batch)
        {
            var descendant = await contentItemRepository.GetByIdAsync(descendantId, cancellationToken);
            if (descendant is not null)
            {
                await ReindexItemAsync(descendant, context, cancellationToken);
            }
        }

        if (descendantIds.Count > BatchSize)
        {
            batchScheduler.ScheduleSubtreeContinuation(contentItem.Id, BatchSize);
        }
    }

    // ContinueSearchIndexBatchJob's entry point for a subtree continuation - re-walks the same subtree
    // (depth is capped at 3, so this is cheap) and resumes from `skip`.
    public async Task ContinueSubtreeBatchAsync(Guid rootContentItemId, int skip, CancellationToken cancellationToken = default)
    {
        var descendantIds = new List<Guid>();
        await CollectDescendantIdsAsync(rootContentItemId, descendantIds, cancellationToken);

        var context = await LoadContextAsync(cancellationToken);
        var batch = descendantIds.Skip(skip).Take(BatchSize).ToList();
        foreach (var descendantId in batch)
        {
            var descendant = await contentItemRepository.GetByIdAsync(descendantId, cancellationToken);
            if (descendant is not null)
            {
                await ReindexItemAsync(descendant, context, cancellationToken);
            }
        }

        if (descendantIds.Count > skip + BatchSize)
        {
            batchScheduler.ScheduleSubtreeContinuation(rootContentItemId, skip + BatchSize);
        }
    }

    public Task RemoveAsync(Guid contentItemId, CancellationToken cancellationToken = default) =>
        searchDocumentRepository.DeleteBySourceAndIdsAsync(WebsiteSourceKey, [contentItemId.ToString()], cancellationToken);

    public Task ReindexContentTypeAsync(Guid contentTypeId, CancellationToken cancellationToken = default) =>
        ContinueContentTypeBatchAsync(contentTypeId, page: 1, cancellationToken);

    // Also ReconcileWebsiteSearchIndexJob's per-type entry point (Görev 2's "zamana bağlı görünürlük"
    // safety net) and ContinueSearchIndexBatchJob's content-type continuation.
    public async Task ContinueContentTypeBatchAsync(Guid contentTypeId, int page, CancellationToken cancellationToken = default)
    {
        var contentType = await contentTypeRepository.GetByIdAsync(contentTypeId, cancellationToken);
        var context = await LoadContextAsync(cancellationToken);
        if (context.DefaultLanguage is null)
        {
            return;
        }

        var currentPage = page;
        for (var pagesProcessed = 0; pagesProcessed < BatchSize / PageSize; pagesProcessed++)
        {
            var result = await contentItemRepository.SearchAsync(
                contentTypeId, status: null, context.DefaultLanguage.Code, requireLanguage: false, search: null,
                isFeatured: null, parentId: null, new PagedRequest { Page = currentPage, PageSize = PageSize }, cancellationToken);

            foreach (var row in result.Items)
            {
                var item = await contentItemRepository.GetByIdAsync(row.Id, cancellationToken);
                if (item is not null)
                {
                    await ReindexItemWithKnownTypeAsync(item, contentType, context, cancellationToken);
                }
            }

            if (!result.HasNextPage)
            {
                return;
            }

            currentPage++;
        }

        batchScheduler.ScheduleContentTypeContinuation(contentTypeId, currentPage);
    }

    public Task RemoveLanguageAsync(LanguageCode languageCode, CancellationToken cancellationToken = default) =>
        ContinueLanguageBatchAsync(languageCode, page: 1, cancellationToken);

    // ContinueSearchIndexBatchJob's language continuation entry point.
    public async Task ContinueLanguageBatchAsync(LanguageCode languageCode, int page, CancellationToken cancellationToken = default)
    {
        var currentPage = page;
        for (var pagesProcessed = 0; pagesProcessed < BatchSize / PageSize; pagesProcessed++)
        {
            var result = await contentItemRepository.SearchAsync(
                contentTypeId: null, status: null, languageCode, requireLanguage: true, search: null, isFeatured: null,
                parentId: null, new PagedRequest { Page = currentPage, PageSize = PageSize }, cancellationToken);

            foreach (var row in result.Items)
            {
                await searchDocumentRepository.DeleteAsync(WebsiteSourceKey, row.Id.ToString(), languageCode, cancellationToken);
            }

            if (!result.HasNextPage)
            {
                return;
            }

            currentPage++;
        }

        batchScheduler.ScheduleLanguageContinuation(languageCode.Value, currentPage);
    }

    private async Task CollectDescendantIdsAsync(Guid parentId, List<Guid> accumulator, CancellationToken cancellationToken)
    {
        var children = await contentItemRepository.GetChildrenAsync(parentId, cancellationToken);
        foreach (var child in children)
        {
            accumulator.Add(child.Id);
            await CollectDescendantIdsAsync(child.Id, accumulator, cancellationToken);
        }
    }

    private async Task<IndexingContext> LoadContextAsync(CancellationToken cancellationToken)
    {
        var activeLanguages = await siteLanguageRepository.GetActiveAsync(cancellationToken);
        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        return new IndexingContext(activeLanguages, defaultLanguage, now);
    }

    private async Task ReindexItemAsync(ContentItem contentItem, IndexingContext context, CancellationToken cancellationToken)
    {
        var contentType = await contentTypeRepository.GetByIdAsync(contentItem.ContentTypeId, cancellationToken);
        await ReindexItemWithKnownTypeAsync(contentItem, contentType, context, cancellationToken);
    }

    private async Task ReindexItemWithKnownTypeAsync(
        ContentItem contentItem, ContentType? contentType, IndexingContext context, CancellationToken cancellationToken)
    {
        if (contentType is null || !contentType.IsSearchable || !contentType.IsActive || !contentType.HasDetailPage
            || context.DefaultLanguage is null)
        {
            await RemoveAsync(contentItem.Id, cancellationToken);
            return;
        }

        var isVisibleWithAncestors = await pathCascadeService.IsVisibleWithAncestorsAsync(contentItem, context.Now, cancellationToken);

        foreach (var translation in contentItem.Translations)
        {
            var isLanguageActive = context.ActiveLanguages.Any(l => l.Code == translation.LanguageCode);
            if (!isVisibleWithAncestors || !isLanguageActive)
            {
                await searchDocumentRepository.DeleteAsync(
                    WebsiteSourceKey, contentItem.Id.ToString(), translation.LanguageCode, cancellationToken);
                continue;
            }

            var url = RoutePathFormat.BuildPublicPath(
                translation.LanguageCode.Value, context.DefaultLanguage.Code.Value, translation.FullPath);
            var summarySource = translation.Summary.Length > 0 ? translation.Summary : translation.Body;
            var summary = SearchTextBuilder.BuildSummary(summarySource, SearchDocument.MaxSummaryLength);
            var normalizedText = SearchTextBuilder.BuildNormalizedText(
                translation.Title, translation.Summary, translation.Body, SearchDocument.MaxNormalizedTextLength);

            var documentResult = SearchDocument.Create(
                WebsiteSourceKey, contentItem.Id.ToString(), translation.LanguageCode, contentType.Key.Value, translation.Title,
                summary, url, normalizedText, contentItem.PublishedAtUtc ?? context.Now, context.Now, !translation.Seo.NoIndex);

            if (documentResult.IsSuccess)
            {
                await searchDocumentRepository.UpsertAsync(documentResult.Value, cancellationToken);
            }
        }
    }

    private sealed record IndexingContext(IReadOnlyList<SiteLanguage> ActiveLanguages, SiteLanguage? DefaultLanguage, DateTime Now);
}
