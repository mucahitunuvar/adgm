namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §11.2 (Faz 4 Görev 3): the full lifecycle an EventRegistration can move through. Only the
// capacity-holding statuses (Confirmed, Attended, NoShow) count against EventSchedule.ConfirmedCount;
// Applied and PendingVerification never do (§1 Faz 4 "kullanıcı kararları"). Attended/NoShow are only
// ever set by Görev 4's admin endpoints (reachable in this Görev's data model, not yet by any command).
public enum EventRegistrationStatus
{
    PendingVerification,
    Applied,
    Confirmed,
    Waitlisted,
    Rejected,
    Cancelled,
    Attended,
    NoShow,
}
