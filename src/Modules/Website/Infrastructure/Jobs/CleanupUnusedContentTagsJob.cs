using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Jobs;

// ADR-024 §4.1 (Faz 1b Görev 3): Hangfire recurring job (Program.cs: RecurringJob.AddOrUpdate<
// CleanupUnusedContentTagsJob> ile günlük kaydedilir), same singleton-safe-dependencies-only pattern
// CleanupStaleNotFoundLogsJob already uses.
public sealed class CleanupUnusedContentTagsJob(IServiceScopeFactory serviceScopeFactory, TimeProvider timeProvider)
{
    private const int UnusedAfterDays = 30;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var contentTagRepository = scope.ServiceProvider.GetRequiredService<IContentTagRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(WebsiteModuleMarker.UnitOfWorkKey);

        var threshold = timeProvider.GetUtcNow().UtcDateTime.AddDays(-UnusedAfterDays);
        var unusedTags = await contentTagRepository.GetUnusedOlderThanAsync(threshold, cancellationToken);

        if (unusedTags.Count == 0)
        {
            return;
        }

        foreach (var tag in unusedTags)
        {
            contentTagRepository.Remove(tag);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
