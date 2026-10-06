namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §11.2 (Faz 4 Görev 3): EventRegistration.Cancel's result shape - tells the command handler
// whether (and which) EventSchedule counter to release, without the handler having to re-derive that
// from the registration's prior Status itself. AlreadyCancelled signals the idempotent no-op path (a
// cancellation link opened twice), where no counter must be touched again.
public enum EventRegistrationCancelOutcome
{
    AlreadyCancelled,
    ReleasedConfirmedSlot,
    ReleasedWaitlistSlot,
    ReleasedNoSlot,
}
