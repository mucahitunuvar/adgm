namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §11.1/§17 (Faz 4 Görev 2): the single place "is this event open for registration" is
// decided - the public events list/detail (this Görev) and the registration command (Görev 3) both
// call this instead of re-deriving the window/capacity logic. Takes primitives, not the EventSchedule
// aggregate itself, so callers can run it against a lightweight projection (no translations, no
// aggregate load) as well as against a fully loaded aggregate's own properties.
public static class EventRegistrationStateResolver
{
    public static EventRegistrationState Resolve(
        bool isCancelled,
        bool registrationEnabled,
        DateTime? registrationOpensAtUtc,
        DateTime? registrationClosesAtUtc,
        DateTime startsAtUtc,
        int? capacity,
        int confirmedCount,
        bool waitlistEnabled,
        DateTime now)
    {
        if (isCancelled)
        {
            return EventRegistrationState.Cancelled;
        }

        if (!registrationEnabled || now >= startsAtUtc || (registrationClosesAtUtc is not null && now > registrationClosesAtUtc.Value))
        {
            return EventRegistrationState.Closed;
        }

        if (registrationOpensAtUtc is not null && now < registrationOpensAtUtc.Value)
        {
            return EventRegistrationState.NotOpen;
        }

        if (capacity is not null && confirmedCount >= capacity.Value)
        {
            return waitlistEnabled ? EventRegistrationState.WaitlistOpen : EventRegistrationState.Full;
        }

        return EventRegistrationState.Open;
    }
}
