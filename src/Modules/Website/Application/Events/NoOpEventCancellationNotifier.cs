using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Events;

// ADR-024 §11.1 (Faz 4 Görev 1): placeholder implementation - see IEventCancellationNotifier's own
// remarks. Görev 4 replaces this registration with the real fan-out.
public sealed class NoOpEventCancellationNotifier : IEventCancellationNotifier
{
    public Task NotifyCancellationAsync(EventSchedule eventSchedule, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
