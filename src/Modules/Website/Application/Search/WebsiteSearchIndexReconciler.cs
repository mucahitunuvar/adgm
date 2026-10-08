using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Application.Search;

// ADR-024 §10 (Faz 5 Görev 4): extracted from ReconcileWebsiteSearchIndexJob (Görev 2) so the exact
// same reconciliation logic can be triggered two ways - the Hangfire recurring job (every tick) and
// the admin "POST .../search/sources/website/reindex" command (on demand) - without duplicating it.
// Adds the SearchSourceState bookkeeping and the SearchSourceSyncCoordinator lock Görev 2's original
// job never needed on its own (nothing could trigger a second, concurrent run of it back then).
public sealed class WebsiteSearchIndexReconciler(
    IContentTypeRepository contentTypeRepository,
    ISearchIndexUpdater searchIndexUpdater,
    ISearchDocumentRepository searchDocumentRepository,
    ISearchSourceStateRepository searchSourceStateRepository,
    SearchSourceSyncCoordinator syncCoordinator,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    public const string WebsiteSourceKey = "website";

    private static readonly Error AlreadyRunningError = Error.Conflict(
        "SearchSource.AlreadyRunning", "A sync for this source is already running.");

    public async Task<Result> ReconcileAsync(CancellationToken cancellationToken = default)
    {
        if (!syncCoordinator.TryEnter(WebsiteSourceKey))
        {
            return Result.Failure(AlreadyRunningError);
        }

        try
        {
            var state = await searchSourceStateRepository.GetOrCreateAsync(WebsiteSourceKey, cancellationToken);
            state.MarkStarted(timeProvider.GetUtcNow().UtcDateTime);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                var contentTypes = await contentTypeRepository.GetAllAsync(cancellationToken);
                foreach (var contentType in contentTypes.Where(t => t.IsSearchable && t.IsActive && t.HasDetailPage))
                {
                    await searchIndexUpdater.ReindexContentTypeAsync(contentType.Id, cancellationToken);
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);

                var documentCount = await searchDocumentRepository.CountBySourceAsync(WebsiteSourceKey, cancellationToken);
                state.MarkSucceeded(timeProvider.GetUtcNow().UtcDateTime, documentCount);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                // Recorded on SearchSourceState, not rethrown - a transient failure here must not crash
                // the recurring job host or turn an admin's on-demand reindex request into a 500; the
                // admin sees the failure via GetSearchSources' LastError instead.
                state.MarkFailed(ex.Message);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
        }
        finally
        {
            syncCoordinator.Exit(WebsiteSourceKey);
        }
    }
}
