using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
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
public sealed class ReconcileWebsiteSearchIndexJob(IServiceScopeFactory serviceScopeFactory)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var contentTypeRepository = scope.ServiceProvider.GetRequiredService<IContentTypeRepository>();
        var searchIndexUpdater = scope.ServiceProvider.GetRequiredService<ISearchIndexUpdater>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(WebsiteModuleMarker.UnitOfWorkKey);

        var contentTypes = await contentTypeRepository.GetAllAsync(cancellationToken);
        foreach (var contentType in contentTypes.Where(t => t.IsSearchable && t.IsActive && t.HasDetailPage))
        {
            await searchIndexUpdater.ReindexContentTypeAsync(contentType.Id, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
