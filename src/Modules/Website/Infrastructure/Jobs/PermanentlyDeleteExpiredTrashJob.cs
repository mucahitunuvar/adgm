using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.ContentPaths;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Jobs;

// ADR-024 §4.5 (Faz 1b Görev 6): Hangfire recurring job (Program.cs: RecurringJob.AddOrUpdate<
// PermanentlyDeleteExpiredTrashJob> ile günlük kaydedilir), same singleton-safe-dependencies-only
// pattern CleanupStaleNotFoundLogsJob/CleanupUnusedContentTagsJob already use.
//
// Only leaf candidates (no children in any status) are deleted in a given run. A candidate with
// children is simply skipped - it becomes eligible once every child is gone, which may happen later in
// this same run (a child deleted earlier in this loop is still present to any query until this run's
// single SaveChangesAsync commits) or, more commonly, in a subsequent day's run. This is exactly the
// master prompt's "job önce en derin öğeleri siler; parent'lar sonraki çalıştırmalarda sırası geldiğinde
// silinir" - achieved by NOT special-casing depth at all, simply by querying the database (not the
// change tracker) for each candidate's children.
public sealed class PermanentlyDeleteExpiredTrashJob(IServiceScopeFactory serviceScopeFactory, TimeProvider timeProvider)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var contentItemRepository = scope.ServiceProvider.GetRequiredService<IContentItemRepository>();
        var permanentDeletionService = scope.ServiceProvider.GetRequiredService<ContentItemPermanentDeletionService>();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(WebsiteModuleMarker.UnitOfWorkKey);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var threshold = now.AddDays(-ContentItem.TrashRetentionDays);
        var candidates = await contentItemRepository.GetTrashedOlderThanAsync(threshold, cancellationToken);

        if (candidates.Count == 0)
        {
            return;
        }

        var deletedAny = false;
        foreach (var candidate in candidates)
        {
            var children = await contentItemRepository.GetChildrenAsync(candidate.Id, cancellationToken);
            if (children.Count > 0)
            {
                continue;
            }

            var result = await permanentDeletionService.DeleteAsync(candidate, Guid.Empty, now, cancellationToken);
            deletedAny = deletedAny || result.IsSuccess;
        }

        if (deletedAny)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            WebsiteCacheInvalidator.InvalidatePublicContent(cacheService);
        }
    }
}
