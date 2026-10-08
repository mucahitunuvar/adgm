using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.Search;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Jobs;

// ADR-024 §10 (Faz 5 Görev 4): Hangfire recurring job (Program.cs:
// RecurringJob.AddOrUpdate<SyncExternalSearchSourcesJob>, default every 10 minutes, configurable via
// Website:Search:ExternalSyncIntervalMinutes) - resolves every registered IExternalSearchSource
// (zero or more; the Host project registers EmployerJobSearchSource as one of them) and runs a full
// sync for each through ExternalSearchSourceSynchronizer. Sources are isolated from one another: one
// throwing (caught inside the synchronizer itself and recorded on its own SearchSourceState) never
// stops the loop from reaching the rest. A project with no IExternalSearchSource registered at all
// simply iterates zero times.
public sealed class SyncExternalSearchSourcesJob(IServiceScopeFactory serviceScopeFactory)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var synchronizer = scope.ServiceProvider.GetRequiredService<ExternalSearchSourceSynchronizer>();
        var sources = scope.ServiceProvider.GetServices<IExternalSearchSource>();

        foreach (var source in sources)
        {
            await synchronizer.SyncAsync(source, cancellationToken);
        }
    }
}
