using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Jobs;

// ADR-024 §4 (Faz 5 Görev 7): Hangfire daily recurring job (Program.cs: RecurringJob.AddOrUpdate<
// PruneContentItemRevisionsJob>), same singleton-safe-dependencies-only pattern every other Website
// job already uses. "İçerik başına en yeni 50 revizyon kalır" - but the newest IsPublishedSnapshot
// revision is always kept too, even when it falls outside that top-50 window, so a long-unedited
// published page never loses the one revision that proves what went live.
public sealed class PruneContentItemRevisionsJob(IServiceScopeFactory serviceScopeFactory)
{
    private const int KeepCount = 50;
    private const int MaxContentItemsPerRun = 500;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IContentItemRevisionRepository>();

        var contentItemIds = await repository.GetContentItemIdsWithMoreThanAsync(KeepCount, MaxContentItemsPerRun, cancellationToken);

        foreach (var contentItemId in contentItemIds)
        {
            var rows = await repository.GetRetentionRowsAsync(contentItemId, cancellationToken);

            var keepIds = rows
                .OrderByDescending(r => r.RevisionNumber)
                .Take(KeepCount)
                .Select(r => r.Id)
                .ToHashSet();

            var newestPublished = rows.Where(r => r.IsPublishedSnapshot).OrderByDescending(r => r.RevisionNumber).FirstOrDefault();
            if (newestPublished is not null)
            {
                keepIds.Add(newestPublished.Id);
            }

            var toDelete = rows.Select(r => r.Id).Where(id => !keepIds.Contains(id)).ToList();
            if (toDelete.Count > 0)
            {
                await repository.DeleteByIdsAsync(toDelete, cancellationToken);
            }
        }
    }
}
