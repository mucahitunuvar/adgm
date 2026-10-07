namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §10 (Faz 5 Görev 2): the Infrastructure-facing half of SearchIndexUpdater's >500-item
// overflow continuation, same split IEventCancellationBatchScheduler already uses for the event-
// cancellation fan-out - kept behind this port so SearchIndexUpdater (Application) never references
// Hangfire or the concrete Infrastructure job type directly (AGENTS.md §7/§28).
public interface ISearchIndexBatchScheduler
{
    void ScheduleSubtreeContinuation(Guid rootContentItemId, int skip);

    void ScheduleContentTypeContinuation(Guid contentTypeId, int page);

    void ScheduleLanguageContinuation(string languageCode, int page);
}
