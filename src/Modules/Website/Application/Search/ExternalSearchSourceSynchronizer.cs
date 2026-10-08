using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Application.Search;

// ADR-024 §10 (Faz 5 Görev 4): full-sync algorithm for exactly one IExternalSearchSource, shared by
// SyncExternalSearchSourcesJob (the recurring job, every registered source) and
// ReindexSearchSourceCommandHandler (the admin "anında yeniden indeksleme" action, one source). Pages
// the source end to end, upserting every document it returns; only deletes the source's unseen
// documents if every page was read without error ("kısmi hatada hiçbir şey silinmez") - a page that
// throws leaves whatever was already upserted in place and simply records the failure.
public sealed class ExternalSearchSourceSynchronizer(
    ISearchDocumentRepository searchDocumentRepository,
    ISearchSourceStateRepository searchSourceStateRepository,
    ISiteLanguageRepository siteLanguageRepository,
    SearchSourceSyncCoordinator syncCoordinator,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    private const int PageSize = 200;

    private static readonly Error AlreadyRunningError = Error.Conflict(
        "SearchSource.AlreadyRunning", "A sync for this source is already running.");

    public async Task<Result> SyncAsync(IExternalSearchSource source, CancellationToken cancellationToken = default)
    {
        if (!syncCoordinator.TryEnter(source.SourceKey))
        {
            return Result.Failure(AlreadyRunningError);
        }

        try
        {
            var state = await searchSourceStateRepository.GetOrCreateAsync(source.SourceKey, cancellationToken);
            state.MarkStarted(timeProvider.GetUtcNow().UtcDateTime);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                // İlanlar (ve bugün için her dış kaynak) tek dillidir: varsayılan site dilinde
                // indekslenir (ADR-024 §10/Görev 4 - ExternalSearchDocument'ın kendi başına bir
                // LanguageCode alanı yok, bu varsayım port'un kendisinde değil burada yaşar).
                var activeLanguages = await siteLanguageRepository.GetActiveAsync(cancellationToken);
                var defaultLanguage = activeLanguages.First(l => l.IsDefault);

                var seenSourceIds = new List<string>();
                var page = 1;
                PagedResult<ExternalSearchDocument> pageResult;
                do
                {
                    pageResult = await source.GetPublishedDocumentsAsync(page, PageSize, cancellationToken);
                    foreach (var document in pageResult.Items)
                    {
                        var normalizedText = SearchTextBuilder.BuildNormalizedText(
                            string.Empty, string.Empty, document.SearchableText, SearchDocument.MaxNormalizedTextLength);
                        var searchDocumentResult = SearchDocument.Create(
                            source.SourceKey, document.SourceId, defaultLanguage.Code, document.TypeKey, document.Title,
                            document.Summary, document.Url, normalizedText, document.PublishedAtUtc,
                            timeProvider.GetUtcNow().UtcDateTime, document.IncludeInSitemap);
                        if (searchDocumentResult.IsFailure)
                        {
                            // A malformed document from the source (e.g. an overlong field) must not
                            // abort the whole sync - skip it, the rest of the source still indexes.
                            continue;
                        }

                        await searchDocumentRepository.UpsertAsync(searchDocumentResult.Value, cancellationToken);
                        seenSourceIds.Add(document.SourceId);
                    }

                    page++;
                }
                while (page <= pageResult.TotalPages);

                await searchDocumentRepository.DeleteUnseenBySourceAsync(source.SourceKey, seenSourceIds, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                var documentCount = await searchDocumentRepository.CountBySourceAsync(source.SourceKey, cancellationToken);
                state.MarkSucceeded(timeProvider.GetUtcNow().UtcDateTime, documentCount);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                // Recorded on SearchSourceState, not rethrown (same reasoning as
                // WebsiteSearchIndexReconciler) - and critically, nothing is deleted on this path: the
                // DeleteUnseenBySourceAsync call above never ran.
                state.MarkFailed(ex.Message);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
        }
        finally
        {
            syncCoordinator.Exit(source.SourceKey);
        }
    }
}
