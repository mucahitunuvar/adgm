using GenclikMerkezi.Modules.Website.Application.Search;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Jobs;

// ADR-024 §10 (Faz 5 Görev 2): "Zamana bağlı görünürlük... bir mutasyon olayı üretmez" - a scheduled
// PublishAtUtc/UnpublishAtUtc crossing "now" changes a ContentItem's effective visibility without any
// handler running, so no ISearchIndexUpdater call happens on its own. This Hangfire recurring job
// (Program.cs: RecurringJob.AddOrUpdate<ReconcileWebsiteSearchIndexJob>, default every 10 minutes) is
// the safety net: re-running SearchIndexUpdater's own per-item visibility check against every
// searchable content type catches up anything a schedule crossing (or any other drift) missed, adding
// newly-visible items and removing newly-invisible ones in the same pass. Only singleton-safe
// dependencies in the constructor, the same self-scoping shape every other Website Hangfire job uses.
//
// Görev 4: the actual reconciliation logic (plus SearchSourceState bookkeeping and the
// SearchSourceSyncCoordinator lock) now lives in WebsiteSearchIndexReconciler, shared with the admin
// "POST .../search/sources/website/reindex" command - this job is just that logic's scheduled trigger.
public sealed class ReconcileWebsiteSearchIndexJob(IServiceScopeFactory serviceScopeFactory)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var reconciler = scope.ServiceProvider.GetRequiredService<WebsiteSearchIndexReconciler>();

        await reconciler.ReconcileAsync(cancellationToken);
    }
}
