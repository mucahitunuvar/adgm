using GenclikMerkezi.Modules.Website.Application.Events;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Jobs;

// ADR-024 §11.2 (Faz 4 Görev 4): the event-cancellation fan-out's overflow continuation for events with
// more than EventCancellationNotifier.BatchSize (200) recipients - enqueued by
// HangfireEventCancellationBatchScheduler, one chunk at a time, each chunk re-enqueuing the next if it
// was itself full. Only singleton-safe dependencies in the constructor (IServiceScopeFactory,
// ILogger<T>), the same self-scoping shape every other Website Hangfire job already uses, since
// Hangfire's own activator in this project never opens a DI scope per job.
public sealed class ProcessEventCancellationNotificationsJob(IServiceScopeFactory serviceScopeFactory)
{
    public async Task ExecuteAsync(Guid contentItemId, int skip, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var notifier = scope.ServiceProvider.GetRequiredService<EventCancellationNotifier>();

        await notifier.ProcessBatchAsync(contentItemId, skip, cancellationToken);
    }
}
