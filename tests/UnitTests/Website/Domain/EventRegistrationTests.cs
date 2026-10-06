using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class EventRegistrationTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LegalDocumentKey PrivacyKey = LegalDocumentKey.Create("kvkk-event").Value;
    private static readonly Guid EventScheduleId = Guid.NewGuid();
    private static readonly Guid ContentItemId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private static Result<EventRegistration> CreateRegistration(
        string? email = "katilimci@example.com",
        string? phone = null,
        Guid? userId = null,
        string? verificationTokenHash = "hash-value",
        DateTime? verificationTokenExpiresAtUtc = null) =>
        EventRegistration.Create(
            EventScheduleId, ContentItemId, "Ahmet", "Yılmaz", email, phone, userId, Tr, PrivacyKey, 1, verificationTokenHash,
            verificationTokenExpiresAtUtc ?? Now.AddHours(24), "cancel-token-value", Now);

    [Fact]
    public void Create_WithValidInput_StartsAsPendingVerification()
    {
        var result = CreateRegistration();

        Assert.True(result.IsSuccess);
        Assert.Equal(EventRegistrationStatus.PendingVerification, result.Value.Status);
        Assert.True(result.Value.IsActive);
        Assert.Single(result.Value.StatusHistory);
        Assert.Null(result.Value.StatusHistory[0].PreviousStatus);
        Assert.Equal(EventRegistrationStatus.PendingVerification, result.Value.StatusHistory[0].NewStatus);
    }

    [Fact]
    public void Create_NormalizesEmailToLowercaseAndTrims()
    {
        var result = CreateRegistration(email: "  Katilimci@Example.com  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("katilimci@example.com", result.Value.Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Create_WithInvalidEmail_Fails(string? email)
    {
        var result = CreateRegistration(email: email);

        Assert.True(result.IsFailure);
        Assert.StartsWith("EventRegistration.Email", result.Error.Code);
    }

    [Fact]
    public void Create_WithPhoneTooLong_Fails()
    {
        var result = CreateRegistration(phone: new string('5', 31));

        Assert.True(result.IsFailure);
        Assert.Equal("EventRegistration.PhoneTooLong", result.Error.Code);
    }

    [Fact]
    public void NormalizeEmail_TrimsAndLowercases()
    {
        var result = EventRegistration.NormalizeEmail("  Visitor@Example.com ");

        Assert.True(result.IsSuccess);
        Assert.Equal("visitor@example.com", result.Value);
    }

    [Theory]
    [InlineData(EventCapacityDecision.Confirmed, EventRegistrationStatus.Confirmed)]
    [InlineData(EventCapacityDecision.Applied, EventRegistrationStatus.Applied)]
    [InlineData(EventCapacityDecision.Waitlisted, EventRegistrationStatus.Waitlisted)]
    public void ApplyCapacityDecision_FromPendingVerification_SetsExpectedStatus(
        EventCapacityDecision decision, EventRegistrationStatus expectedStatus)
    {
        var registration = CreateRegistration().Value;

        var result = registration.ApplyCapacityDecision(decision, "System", Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedStatus, registration.Status);
        Assert.NotNull(registration.VerifiedAtUtc);
        Assert.Equal(2, registration.StatusHistory.Count);
    }

    // ADR-024 §11.2 (Faz 4 Görev 3): EventCapacityConcurrencyRetryExecutor may call this more than
    // once against the same in-memory instance across a RowVersion-conflict retry - each call must
    // undo the previous (never-persisted) attempt's decision rather than accumulate history entries.
    [Fact]
    public void ApplyCapacityDecision_CalledTwice_OnlyKeepsTheFinalDecision()
    {
        var registration = CreateRegistration().Value;

        registration.ApplyCapacityDecision(EventCapacityDecision.Confirmed, "System", Now);
        var secondResult = registration.ApplyCapacityDecision(EventCapacityDecision.Waitlisted, "System", Now.AddSeconds(1));

        Assert.True(secondResult.IsSuccess);
        Assert.Equal(EventRegistrationStatus.Waitlisted, registration.Status);
        Assert.Equal(2, registration.StatusHistory.Count);
        Assert.Equal(EventRegistrationStatus.Waitlisted, registration.StatusHistory[1].NewStatus);
    }

    [Fact]
    public void ApplyCapacityDecision_AfterConfirmCapacityDecisionCommitted_StillOverwritesOnNextCall()
    {
        var registration = CreateRegistration().Value;
        registration.ApplyCapacityDecision(EventCapacityDecision.Confirmed, "System", Now);
        registration.ConfirmCapacityDecisionCommitted();

        var result = registration.ApplyCapacityDecision(EventCapacityDecision.Waitlisted, "System", Now);

        Assert.True(result.IsFailure);
        Assert.Equal("EventRegistration.NotPendingVerification", result.Error.Code);
    }

    [Fact]
    public void CanSendVerificationEmail_WithinCooldown_ReturnsFalse()
    {
        var registration = CreateRegistration().Value;
        registration.RecordVerificationEmailSent(Now);

        Assert.False(registration.CanSendVerificationEmail(Now.AddMinutes(5)));
        Assert.True(registration.CanSendVerificationEmail(Now.AddMinutes(10)));
    }

    [Fact]
    public void ReissueVerificationToken_WhenNotPendingVerification_Fails()
    {
        var registration = CreateRegistration().Value;
        registration.ApplyCapacityDecision(EventCapacityDecision.Confirmed, "System", Now);

        var result = registration.ReissueVerificationToken("new-hash", Now.AddHours(24), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("EventRegistration.NotPendingVerification", result.Error.Code);
    }

    [Theory]
    [InlineData(EventRegistrationStatus.Confirmed, EventRegistrationCancelOutcome.ReleasedConfirmedSlot)]
    [InlineData(EventRegistrationStatus.Waitlisted, EventRegistrationCancelOutcome.ReleasedWaitlistSlot)]
    [InlineData(EventRegistrationStatus.Applied, EventRegistrationCancelOutcome.ReleasedNoSlot)]
    public void Cancel_FromCapacityHoldingOrNotStatus_ReturnsExpectedOutcome(
        EventRegistrationStatus fromStatus, EventRegistrationCancelOutcome expectedOutcome)
    {
        var registration = CreateRegistration().Value;
        var decision = fromStatus switch
        {
            EventRegistrationStatus.Confirmed => EventCapacityDecision.Confirmed,
            EventRegistrationStatus.Waitlisted => EventCapacityDecision.Waitlisted,
            _ => EventCapacityDecision.Applied,
        };
        registration.ApplyCapacityDecision(decision, "System", Now);

        var result = registration.Cancel(EventRegistrationCancelledBy.Participant, Now.AddMinutes(5));

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedOutcome, result.Value);
        Assert.Equal(EventRegistrationStatus.Cancelled, registration.Status);
        Assert.Equal(EventRegistrationCancelledBy.Participant, registration.CancelledBy);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_IsIdempotent()
    {
        var registration = CreateRegistration().Value;
        registration.Cancel(EventRegistrationCancelledBy.Participant, Now);

        var result = registration.Cancel(EventRegistrationCancelledBy.Participant, Now.AddMinutes(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(EventRegistrationCancelOutcome.AlreadyCancelled, result.Value);
    }

    [Fact]
    public void Cancel_FromPendingVerification_ReleasesNoSlot()
    {
        var registration = CreateRegistration().Value;

        var result = registration.Cancel(EventRegistrationCancelledBy.Participant, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(EventRegistrationCancelOutcome.ReleasedNoSlot, result.Value);
    }
}
