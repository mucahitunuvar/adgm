using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.Events;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.WaitlistEventRegistration;

// ADR-024 §11.2 (Faz 4 Görev 4): the admin "move to waitlist" decision - only valid while
// EventSchedule.WaitlistEnabled is on and the registration is currently Applied (§1 "yalnızca
// WaitlistEnabled ve Applied"). No participant-facing email is defined for this action in §1 (only
// confirm/reject/admin-cancel have one) - a deliberate omission, not an oversight; see the Görev 4
// completion report.
public sealed class WaitlistEventRegistrationCommandHandler(
    IEventRegistrationRepository eventRegistrationRepository,
    IEventScheduleRepository eventScheduleRepository,
    EventCapacityConcurrencyRetryExecutor capacityRetryExecutor,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider)
    : IRequestHandler<WaitlistEventRegistrationCommand, Result>
{
    private static readonly Error NotFoundError = Error.NotFound(
        "EventRegistration.NotFound", "This registration could not be found.");

    private static readonly Error ConcurrencyError = Error.Conflict(
        "EventRegistration.ConcurrencyConflict", "The registration was changed by someone else. Reload and try again.");

    private static readonly Error WaitlistDisabledError = Error.Conflict(
        "Event.WaitlistDisabled", "Waitlisting is not enabled for this event.");

    public async Task<Result> Handle(WaitlistEventRegistrationCommand request, CancellationToken cancellationToken)
    {
        var registration = await eventRegistrationRepository.GetByIdAsync(request.Id, cancellationToken);
        if (registration is null || registration.ContentItemId != request.ContentItemId)
        {
            return Result.Failure(NotFoundError);
        }

        if (!request.RowVersion.SequenceEqual(registration.RowVersion))
        {
            return Result.Failure(ConcurrencyError);
        }

        var eventSchedule = await eventScheduleRepository.GetByContentItemIdAsync(request.ContentItemId, cancellationToken);
        if (eventSchedule is null)
        {
            return Result.Failure(NotFoundError);
        }

        if (!eventSchedule.WaitlistEnabled)
        {
            return Result.Failure(WaitlistDisabledError);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var changedBy = currentUserContext.UserId!.Value.ToString();

        var result = await capacityRetryExecutor.ExecuteAsync(
            eventSchedule,
            () =>
            {
                // AdminWaitlist validates the registration's own status first - only touch
                // EventSchedule's counter once that succeeds, so a rejected transition never leaves a
                // stray WaitlistedCount increment behind.
                var waitlistResult = registration.AdminWaitlist(changedBy, now);
                if (waitlistResult.IsFailure)
                {
                    return waitlistResult;
                }

                eventSchedule.AddToWaitlist();
                return Result.Success();
            },
            cancellationToken);

        if (result.IsFailure)
        {
            return result;
        }

        registration.ConfirmCapacityDecisionCommitted();

        return Result.Success();
    }
}
