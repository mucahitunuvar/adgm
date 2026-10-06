using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

// ADR-024 §11.1/§17 (Faz 4 Görev 2): table test for the single pure function GetPublicEvents, the
// public content detail endpoint and (Görev 3) the registration command all share.
public class EventRegistrationStateResolverTests
{
    private static readonly DateTime Now = new(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Resolve_Cancelled_TakesPriorityOverEverythingElse()
    {
        var state = EventRegistrationStateResolver.Resolve(
            isCancelled: true, registrationEnabled: true, registrationOpensAtUtc: null, registrationClosesAtUtc: null,
            startsAtUtc: Now.AddDays(1), capacity: 10, confirmedCount: 20, waitlistEnabled: true, now: Now);

        Assert.Equal(EventRegistrationState.Cancelled, state);
    }

    [Fact]
    public void Resolve_RegistrationDisabled_ReturnsClosed()
    {
        var state = EventRegistrationStateResolver.Resolve(
            isCancelled: false, registrationEnabled: false, registrationOpensAtUtc: null, registrationClosesAtUtc: null,
            startsAtUtc: Now.AddDays(1), capacity: null, confirmedCount: 0, waitlistEnabled: false, now: Now);

        Assert.Equal(EventRegistrationState.Closed, state);
    }

    [Fact]
    public void Resolve_EventAlreadyStarted_ReturnsClosed()
    {
        var state = EventRegistrationStateResolver.Resolve(
            isCancelled: false, registrationEnabled: true, registrationOpensAtUtc: null, registrationClosesAtUtc: null,
            startsAtUtc: Now.AddMinutes(-1), capacity: null, confirmedCount: 0, waitlistEnabled: false, now: Now);

        Assert.Equal(EventRegistrationState.Closed, state);
    }

    [Fact]
    public void Resolve_PastRegistrationClosesAtUtc_ReturnsClosed()
    {
        var state = EventRegistrationStateResolver.Resolve(
            isCancelled: false, registrationEnabled: true, registrationOpensAtUtc: null, registrationClosesAtUtc: Now.AddMinutes(-1),
            startsAtUtc: Now.AddDays(1), capacity: null, confirmedCount: 0, waitlistEnabled: false, now: Now);

        Assert.Equal(EventRegistrationState.Closed, state);
    }

    [Fact]
    public void Resolve_BeforeRegistrationOpensAtUtc_ReturnsNotOpen()
    {
        var state = EventRegistrationStateResolver.Resolve(
            isCancelled: false, registrationEnabled: true, registrationOpensAtUtc: Now.AddMinutes(1), registrationClosesAtUtc: null,
            startsAtUtc: Now.AddDays(1), capacity: null, confirmedCount: 0, waitlistEnabled: false, now: Now);

        Assert.Equal(EventRegistrationState.NotOpen, state);
    }

    [Fact]
    public void Resolve_UnlimitedCapacity_ReturnsOpen()
    {
        var state = EventRegistrationStateResolver.Resolve(
            isCancelled: false, registrationEnabled: true, registrationOpensAtUtc: null, registrationClosesAtUtc: null,
            startsAtUtc: Now.AddDays(1), capacity: null, confirmedCount: 1_000_000, waitlistEnabled: false, now: Now);

        Assert.Equal(EventRegistrationState.Open, state);
    }

    [Fact]
    public void Resolve_CapacityAvailable_ReturnsOpen()
    {
        var state = EventRegistrationStateResolver.Resolve(
            isCancelled: false, registrationEnabled: true, registrationOpensAtUtc: null, registrationClosesAtUtc: null,
            startsAtUtc: Now.AddDays(1), capacity: 10, confirmedCount: 9, waitlistEnabled: false, now: Now);

        Assert.Equal(EventRegistrationState.Open, state);
    }

    [Fact]
    public void Resolve_CapacityFullWithoutWaitlist_ReturnsFull()
    {
        var state = EventRegistrationStateResolver.Resolve(
            isCancelled: false, registrationEnabled: true, registrationOpensAtUtc: null, registrationClosesAtUtc: null,
            startsAtUtc: Now.AddDays(1), capacity: 10, confirmedCount: 10, waitlistEnabled: false, now: Now);

        Assert.Equal(EventRegistrationState.Full, state);
    }

    [Fact]
    public void Resolve_CapacityFullWithWaitlist_ReturnsWaitlistOpen()
    {
        var state = EventRegistrationStateResolver.Resolve(
            isCancelled: false, registrationEnabled: true, registrationOpensAtUtc: null, registrationClosesAtUtc: null,
            startsAtUtc: Now.AddDays(1), capacity: 10, confirmedCount: 10, waitlistEnabled: true, now: Now);

        Assert.Equal(EventRegistrationState.WaitlistOpen, state);
    }

    [Fact]
    public void Resolve_CapacityOverfilled_StillReturnsWaitlistOpen()
    {
        // Defensive: ConfirmedCount should never exceed Capacity (domain invariant), but >= rather than
        // == keeps the resolver correct even if that invariant were ever violated.
        var state = EventRegistrationStateResolver.Resolve(
            isCancelled: false, registrationEnabled: true, registrationOpensAtUtc: null, registrationClosesAtUtc: null,
            startsAtUtc: Now.AddDays(1), capacity: 10, confirmedCount: 11, waitlistEnabled: true, now: Now);

        Assert.Equal(EventRegistrationState.WaitlistOpen, state);
    }

    [Fact]
    public void Resolve_OpensAtUtcInThePast_DoesNotReturnNotOpen()
    {
        var state = EventRegistrationStateResolver.Resolve(
            isCancelled: false, registrationEnabled: true, registrationOpensAtUtc: Now.AddMinutes(-1), registrationClosesAtUtc: null,
            startsAtUtc: Now.AddDays(1), capacity: null, confirmedCount: 0, waitlistEnabled: false, now: Now);

        Assert.Equal(EventRegistrationState.Open, state);
    }
}
