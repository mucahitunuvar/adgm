using GenclikMerkezi.Modules.Website.Application.Abstractions;
using Hangfire;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Jobs;

// ADR-024 §11.2 (Faz 4 Görev 4): IEventCancellationBatchScheduler's only implementation - the one place
// in this module's cancellation fan-out that touches Hangfire directly, keeping
// EventCancellationNotifier (Application) free of any Infrastructure/third-party dependency
// (AGENTS.md §7/§28).
public sealed class HangfireEventCancellationBatchScheduler(IBackgroundJobClient backgroundJobClient)
    : IEventCancellationBatchScheduler
{
    public void ScheduleNextBatch(Guid contentItemId, int skip) =>
        backgroundJobClient.Enqueue<ProcessEventCancellationNotificationsJob>(
            job => job.ExecuteAsync(contentItemId, skip, CancellationToken.None));
}
