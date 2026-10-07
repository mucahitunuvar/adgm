using GenclikMerkezi.Modules.Website.Application.Search;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Jobs;

// ADR-024 §10 (Faz 5 Görev 2): the search index's overflow continuation for a subtree/content-type/
// language batch larger than SearchIndexUpdater.BatchSize (500) - enqueued by
// HangfireSearchIndexBatchScheduler, one chunk at a time, each chunk re-enqueuing the next if it was
// itself full. Only singleton-safe dependencies in the constructor (IServiceScopeFactory), the same
// self-scoping shape every other Website Hangfire job already uses.
public sealed class ContinueSearchIndexBatchJob(IServiceScopeFactory serviceScopeFactory)
{
    public async Task ContinueSubtreeAsync(Guid rootContentItemId, int skip, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var updater = scope.ServiceProvider.GetRequiredService<SearchIndexUpdater>();
        await updater.ContinueSubtreeBatchAsync(rootContentItemId, skip, cancellationToken);
        await SaveChangesAsync(scope, cancellationToken);
    }

    public async Task ContinueContentTypeAsync(Guid contentTypeId, int page, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var updater = scope.ServiceProvider.GetRequiredService<SearchIndexUpdater>();
        await updater.ContinueContentTypeBatchAsync(contentTypeId, page, cancellationToken);
        await SaveChangesAsync(scope, cancellationToken);
    }

    public async Task ContinueLanguageAsync(string languageCode, int page, CancellationToken cancellationToken = default)
    {
        var languageCodeResult = LanguageCode.Create(languageCode);
        if (languageCodeResult.IsFailure)
        {
            return;
        }

        using var scope = serviceScopeFactory.CreateScope();
        var updater = scope.ServiceProvider.GetRequiredService<SearchIndexUpdater>();
        await updater.ContinueLanguageBatchAsync(languageCodeResult.Value, page, cancellationToken);
        await SaveChangesAsync(scope, cancellationToken);
    }

    private static Task SaveChangesAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(WebsiteModuleMarker.UnitOfWorkKey);
        return unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
