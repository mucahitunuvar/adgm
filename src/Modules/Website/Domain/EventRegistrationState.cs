namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §11.1/§17 (Faz 4 Görev 2): the public-facing registration state for an EventSchedule -
// never stored, always computed by EventRegistrationStateResolver. NotOpen and Closed both mean "not
// registrable yet/anymore" but are kept distinct so the frontend can show "coming soon" vs "closed"
// without re-deriving the window logic itself.
public enum EventRegistrationState
{
    NotOpen,
    Open,
    Full,
    WaitlistOpen,
    Closed,
    Cancelled,
}
