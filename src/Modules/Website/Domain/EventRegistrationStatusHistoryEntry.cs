using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §11.2 (Faz 4 Görev 3): "her geçiş StatusHistory'ye (zaman, kim, eski/yeni) yazılır" - an
// owned, append-only audit trail of every status transition EventRegistration.Cancel/
// ApplyCapacityDecision/MarkPendingVerification perform. ChangedBy is a free-text actor label
// ("System", "Participant", an admin's UserId.ToString()) rather than a typed reference, mirroring
// RedirectAudit's own "who" column - this history is for display/audit, not for driving further logic.
public sealed class EventRegistrationStatusHistoryEntry : Entity
{
    public EventRegistrationStatus? PreviousStatus { get; private set; }

    public EventRegistrationStatus NewStatus { get; private set; }

    public string ChangedBy { get; private set; } = string.Empty;

    public DateTime OccurredAtUtc { get; private set; }

    private EventRegistrationStatusHistoryEntry(
        Guid id, EventRegistrationStatus? previousStatus, EventRegistrationStatus newStatus, string changedBy, DateTime occurredAtUtc)
        : base(id)
    {
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        ChangedBy = changedBy;
        OccurredAtUtc = occurredAtUtc;
    }

    private EventRegistrationStatusHistoryEntry()
    {
    }

    internal static EventRegistrationStatusHistoryEntry Create(
        EventRegistrationStatus? previousStatus, EventRegistrationStatus newStatus, string changedBy, DateTime occurredAtUtc) =>
        new(Guid.NewGuid(), previousStatus, newStatus, changedBy, occurredAtUtc);
}
