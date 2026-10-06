using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.Events;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.VerifyEventRegistration;

// ADR-024 §11.2 (Faz 4 Görev 3): the deferred half of the "kontenjan kararı" for an anonymous
// registrant - EventRegistration.ApplyCapacityDecision runs here instead of at creation time (see
// CreateEventRegistrationCommandHandler's own remarks). Idempotent for an already-decided
// registration (a verification link opened twice): the second click's lookup still finds the row (its
// hash is never cleared) but Status has already moved past PendingVerification, so this simply
// returns success without touching the counters again.
public sealed class VerifyEventRegistrationCommandHandler(
    IEventRegistrationRepository eventRegistrationRepository,
    IEventScheduleRepository eventScheduleRepository,
    IContentItemRepository contentItemRepository,
    EventCapacityConcurrencyRetryExecutor capacityRetryExecutor,
    EventRegistrationNotifier notifier,
    TimeProvider timeProvider)
    : IRequestHandler<VerifyEventRegistrationCommand, Result>
{
    private static readonly Error InvalidOrExpiredTokenError = Error.Validation(
        "EventRegistration.InvalidVerificationToken", "Invalid or expired verification token.");

    public async Task<Result> Handle(VerifyEventRegistrationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return Result.Failure(InvalidOrExpiredTokenError);
        }

        var hash = EventRegistrationTokens.Hash(request.Token);
        var registration = await eventRegistrationRepository.GetByVerificationTokenHashAsync(hash, cancellationToken);
        if (registration is null)
        {
            return Result.Failure(InvalidOrExpiredTokenError);
        }

        if (registration.Status != EventRegistrationStatus.PendingVerification)
        {
            return Result.Success();
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (registration.VerificationTokenExpiresAtUtc is null || now > registration.VerificationTokenExpiresAtUtc.Value)
        {
            return Result.Failure(InvalidOrExpiredTokenError);
        }

        var eventSchedule = await eventScheduleRepository.GetByContentItemIdAsync(registration.ContentItemId, cancellationToken);
        if (eventSchedule is null)
        {
            return Result.Failure(InvalidOrExpiredTokenError);
        }

        var registrationState = EventRegistrationStateResolver.Resolve(
            eventSchedule.IsCancelled, eventSchedule.RegistrationEnabled, eventSchedule.RegistrationOpensAtUtc,
            eventSchedule.RegistrationClosesAtUtc, eventSchedule.StartsAtUtc, eventSchedule.Capacity, eventSchedule.ConfirmedCount,
            eventSchedule.WaitlistEnabled, now);

        // §1 "Etkinlik iptal/kapalıysa reddet" - Full is deliberately NOT rejected here: that is
        // ReserveCapacity's own job (waitlist-or-409 decision) below, same resolver, different caller.
        var stateError = registrationState switch
        {
            EventRegistrationState.Cancelled => Error.Conflict("Event.Cancelled", "This event has been cancelled."),
            EventRegistrationState.NotOpen or EventRegistrationState.Closed => Error.Conflict(
                "Event.RegistrationClosed", "Registration for this event is not open."),
            _ => (Error?)null,
        };
        if (stateError is not null)
        {
            return Result.Failure(stateError);
        }

        var reserveOutcome = await capacityRetryExecutor.ExecuteAsync(
            eventSchedule,
            () =>
            {
                var reserveResult = eventSchedule.ReserveCapacity();
                return reserveResult.IsFailure
                    ? Result.Failure(reserveResult.Error)
                    : registration.ApplyCapacityDecision(reserveResult.Value, "System", now);
            },
            cancellationToken);

        if (reserveOutcome.IsFailure)
        {
            // §1 "kayıt Rejected olmaz, silinme job'ına bırakılır" - nothing above this point is
            // persisted (no SaveChangesAsync has run), so the row is left exactly as PendingVerification
            // for Görev 5's 24-hour cleanup job to eventually remove.
            return reserveOutcome;
        }

        registration.ConfirmCapacityDecisionCommitted();

        var contentItem = await contentItemRepository.GetByIdAsync(registration.ContentItemId, cancellationToken);
        var eventTitle = contentItem?.Translations.FirstOrDefault(t => t.LanguageCode == registration.LanguageCode)?.Title
            ?? contentItem?.Translations.FirstOrDefault()?.Title ?? string.Empty;

        await notifier.SendCapacityDecisionEmailAsync(registration, eventSchedule, eventTitle, cancellationToken);

        return Result.Success();
    }
}
