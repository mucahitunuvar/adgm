namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §11.2 (Faz 4 Görev 4): the Infrastructure-facing half of the event-cancellation fan-out's
// >200-recipient overflow continuation (§1 "fazlası için Hangfire job'ı ile parçalanır") - kept behind
// this port so EventCancellationNotifier (Application) never references Hangfire or the concrete
// Infrastructure job type directly (AGENTS.md §7/§28: background-job scheduling is an Infrastructure
// concern, same category as RabbitMQ).
public interface IEventCancellationBatchScheduler
{
    void ScheduleNextBatch(Guid contentItemId, int skip);
}
