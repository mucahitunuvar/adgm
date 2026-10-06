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

    // Faz 4 Görev 4: AdminConfirm is valid from Applied (plain approval) or Waitlisted (promotion);
    // every other source status is rejected.
    [Theory]
    [InlineData(EventRegistrationStatus.Applied, true)]
    [InlineData(EventRegistrationStatus.Waitlisted, true)]
    [InlineData(EventRegistrationStatus.PendingVerification, false)]
    [InlineData(EventRegistrationStatus.Confirmed, false)]
    [InlineData(EventRegistrationStatus.Rejected, false)]
    [InlineData(EventRegistrationStatus.Cancelled, false)]
    [InlineData(EventRegistrationStatus.Attended, false)]
    [InlineData(EventRegistrationStatus.NoShow, false)]
    public void AdminConfirm_FromVariousStatuses_SucceedsOnlyFromApplicableOrWaitlisted(
        EventRegistrationStatus fromStatus, bool expectedSuccess)
    {
        var registration = MoveTo(fromStatus);

        var result = registration.AdminConfirm("admin-1", Now.AddHours(1));

        Assert.Equal(expectedSuccess, result.IsSuccess);
        if (expectedSuccess)
        {
            Assert.Equal(EventRegistrationStatus.Confirmed, registration.Status);
        }
        else
        {
            Assert.Equal("EventRegistration.InvalidStatusTransition", result.Error.Code);
        }
    }

    // Mirrors ApplyCapacityDecision_CalledTwice_OnlyKeepsTheFinalDecision - EventCapacityConcurrencyRetryExecutor
    // re-invokes AdminConfirm once per retry attempt, so a second call must undo the first's
    // never-persisted history entry rather than accumulate.
    [Fact]
    public void AdminConfirm_CalledTwice_OnlyKeepsOneHistoryEntryAndRestoresOnUndo()
    {
        var registration = MoveTo(EventRegistrationStatus.Applied);
        var historyCountBefore = registration.StatusHistory.Count;

        registration.AdminConfirm("admin-1", Now.AddHours(1));
        var secondResult = registration.AdminConfirm("admin-1", Now.AddHours(1).AddSeconds(1));

        Assert.True(secondResult.IsSuccess);
        Assert.Equal(EventRegistrationStatus.Confirmed, registration.Status);
        Assert.Equal(historyCountBefore + 1, registration.StatusHistory.Count);
    }

    [Theory]
    [InlineData(EventRegistrationStatus.Applied, true)]
    [InlineData(EventRegistrationStatus.Waitlisted, false)]
    [InlineData(EventRegistrationStatus.Confirmed, false)]
    [InlineData(EventRegistrationStatus.PendingVerification, false)]
    public void AdminWaitlist_FromVariousStatuses_SucceedsOnlyFromApplied(EventRegistrationStatus fromStatus, bool expectedSuccess)
    {
        var registration = MoveTo(fromStatus);

        var result = registration.AdminWaitlist("admin-1", Now.AddHours(1));

        Assert.Equal(expectedSuccess, result.IsSuccess);
        if (expectedSuccess)
        {
            Assert.Equal(EventRegistrationStatus.Waitlisted, registration.Status);
            Assert.NotNull(registration.WaitlistedAtUtc);
        }
        else
        {
            Assert.Equal("EventRegistration.InvalidStatusTransition", result.Error.Code);
        }
    }

    [Theory]
    [InlineData(EventRegistrationStatus.Applied, EventRegistrationCancelOutcome.ReleasedNoSlot)]
    [InlineData(EventRegistrationStatus.Waitlisted, EventRegistrationCancelOutcome.ReleasedWaitlistSlot)]
    public void Reject_FromApplicableOrWaitlisted_ReturnsExpectedOutcome(
        EventRegistrationStatus fromStatus, EventRegistrationCancelOutcome expectedOutcome)
    {
        var registration = MoveTo(fromStatus);

        var result = registration.Reject("admin-1", Now.AddHours(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedOutcome, result.Value);
        Assert.Equal(EventRegistrationStatus.Rejected, registration.Status);
    }

    [Theory]
    [InlineData(EventRegistrationStatus.Confirmed)]
    [InlineData(EventRegistrationStatus.PendingVerification)]
    public void Reject_FromNonApplicableStatuses_Fails(EventRegistrationStatus fromStatus)
    {
        var registration = MoveTo(fromStatus);

        var result = registration.Reject("admin-1", Now.AddHours(1));

        Assert.True(result.IsFailure);
        Assert.Equal("EventRegistration.InvalidStatusTransition", result.Error.Code);
    }

    [Fact]
    public void MarkAttended_FromConfirmed_Succeeds()
    {
        var registration = MoveTo(EventRegistrationStatus.Confirmed);

        var result = registration.MarkAttended("admin-1", Now.AddHours(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(EventRegistrationStatus.Attended, registration.Status);
    }

    [Fact]
    public void MarkNoShow_FromConfirmed_Succeeds()
    {
        var registration = MoveTo(EventRegistrationStatus.Confirmed);

        var result = registration.MarkNoShow("admin-1", Now.AddHours(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(EventRegistrationStatus.NoShow, registration.Status);
    }

    [Theory]
    [InlineData(EventRegistrationStatus.Applied)]
    [InlineData(EventRegistrationStatus.Waitlisted)]
    [InlineData(EventRegistrationStatus.PendingVerification)]
    public void MarkAttended_FromNonConfirmedStatuses_Fails(EventRegistrationStatus fromStatus)
    {
        var registration = MoveTo(fromStatus);

        var result = registration.MarkAttended("admin-1", Now.AddHours(1));

        Assert.True(result.IsFailure);
        Assert.Equal("EventRegistration.InvalidStatusTransition", result.Error.Code);
    }

    // Drives a fresh registration to the requested status via its own public transitions, so every
    // table test above exercises the real domain methods rather than reflection/internal setters.
    private static EventRegistration MoveTo(EventRegistrationStatus status)
    {
        var registration = CreateRegistration().Value;
        if (status == EventRegistrationStatus.PendingVerification)
        {
            return registration;
        }

        if (status is EventRegistrationStatus.Attended or EventRegistrationStatus.NoShow)
        {
            registration.ApplyCapacityDecision(EventCapacityDecision.Confirmed, "System", Now);
            registration.ConfirmCapacityDecisionCommitted();
            if (status == EventRegistrationStatus.Attended)
            {
                registration.MarkAttended("admin-1", Now);
            }
            else
            {
                registration.MarkNoShow("admin-1", Now);
            }

            return registration;
        }

        if (status == EventRegistrationStatus.Rejected)
        {
            registration.ApplyCapacityDecision(EventCapacityDecision.Applied, "System", Now);
            registration.ConfirmCapacityDecisionCommitted();
            registration.Reject("admin-1", Now);
            return registration;
        }

        if (status == EventRegistrationStatus.Cancelled)
        {
            registration.Cancel(EventRegistrationCancelledBy.Participant, Now);
            return registration;
        }

        var decision = status switch
        {
            EventRegistrationStatus.Confirmed => EventCapacityDecision.Confirmed,
            EventRegistrationStatus.Waitlisted => EventCapacityDecision.Waitlisted,
            _ => EventCapacityDecision.Applied,
        };
        registration.ApplyCapacityDecision(decision, "System", Now);
        registration.ConfirmCapacityDecisionCommitted();

        return registration;
    }
}
