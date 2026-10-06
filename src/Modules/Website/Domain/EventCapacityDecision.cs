namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §11.2 (Faz 4 Görev 3): the outcome of EventSchedule.ReserveCapacity - which status an
// EventRegistration receives once it is (or becomes, for an anonymous registrant) verified. Kept as
// its own enum rather than reusing EventRegistrationStatus directly: ReserveCapacity only ever
// produces one of these three values, never PendingVerification/Rejected/Cancelled/Attended/NoShow,
// and a narrower return type makes that explicit at the call site.
public enum EventCapacityDecision
{
    Applied,
    Confirmed,
    Waitlisted,
}
