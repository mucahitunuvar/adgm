namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §11.2 (Faz 4 Görev 3): who cancelled an EventRegistration. Event is reserved for the
// "Etkinlik iptal edildiğinde kayıtların durumu değişmez" rule (Görev 4's cancellation fan-out never
// actually transitions a registration to Cancelled - the event itself being cancelled is read off
// EventSchedule.IsCancelled instead), so it is modeled here for data-shape completeness but, by
// design, Cancel never assigns it.
public enum EventRegistrationCancelledBy
{
    Participant,
    Admin,
    Event,
}
