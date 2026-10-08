using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.Search;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ReindexSearchSource;

// ADR-024 §10 (Faz 5 Görev 4): "anında yeniden indeksleme" - runs the same sync logic the recurring
// jobs use (WebsiteSearchIndexReconciler for "website", ExternalSearchSourceSynchronizer for every
// other registered source), synchronously, within this request. Both share SearchSourceSyncCoordinator
// with the recurring jobs, so a sync already running (from either trigger) surfaces here as 409
// instead of running twice.
public sealed class ReindexSearchSourceCommandHandler(
    IEnumerable<IExternalSearchSource> externalSearchSources,
    ExternalSearchSourceSynchronizer externalSearchSourceSynchronizer,
    WebsiteSearchIndexReconciler websiteSearchIndexReconciler)
    : IRequestHandler<ReindexSearchSourceCommand, Result>
{
    private static readonly Error NotFoundError = Error.NotFound(
        "SearchSource.NotFound", "The specified search source could not be found.");

    public Task<Result> Handle(ReindexSearchSourceCommand request, CancellationToken cancellationToken)
    {
        if (request.SourceKey == WebsiteSearchIndexReconciler.WebsiteSourceKey)
        {
            return websiteSearchIndexReconciler.ReconcileAsync(cancellationToken);
        }

        var source = externalSearchSources.FirstOrDefault(s => s.SourceKey == request.SourceKey);

        return source is null
            ? Task.FromResult(Result.Failure(NotFoundError))
            : externalSearchSourceSynchronizer.SyncAsync(source, cancellationToken);
    }
}
