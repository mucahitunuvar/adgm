using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §11.1 (Faz 4 Görev 1): CancelEventScheduleCommandHandler's post-commit, best-effort fan-out
// to every active registration (Applied/Confirmed/Waitlisted) once an event is cancelled. The real
// implementation needs EventRegistration, which does not exist until Görev 3 - this Görev wires the
// port and calls it with a no-op implementation (NoOpEventCancellationNotifier) so Görev 4 only has to
// add the real implementation, not touch the handler that calls it.
public interface IEventCancellationNotifier
{
    Task NotifyCancellationAsync(EventSchedule eventSchedule, CancellationToken cancellationToken = default);
}
