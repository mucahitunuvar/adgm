using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.Events;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.CancelEventRegistration;

// ADR-024 §11.2 (Faz 4 Görev 3). EventRegistration.Cancel runs once, outside the retry loop - its
// outcome (which, if any, EventSchedule counter to release) does not depend on a fresh schedule read,
// only on the registration's own status, so there is nothing to redo if the counter-release's
// SaveChangesAsync hits a RowVersion conflict. Only the counter release itself (naturally
// re-appliable against a freshly reloaded schedule - see EventSchedule.ReleaseConfirmedSlot/
// ReleaseWaitlistSlot) goes through EventCapacityConcurrencyRetryExecutor.
public sealed class CancelEventRegistrationCommandHandler(
    IEventRegistrationRepository eventRegistrationRepository,
    IEventScheduleRepository eventScheduleRepository,
    EventCapacityConcurrencyRetryExecutor capacityRetryExecutor,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CancelEventRegistrationCommand, Result>
{
    private static readonly Error InvalidTokenError = Error.Validation(
        "EventRegistration.InvalidCancelToken", "Invalid cancellation token.");

    private static readonly Error AlreadyStartedError = Error.Conflict(
        "Event.AlreadyStarted", "This event has already started; the registration can no longer be cancelled.");

    public async Task<Result> Handle(CancelEventRegistrationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return Result.Failure(InvalidTokenError);
        }

        var registration = await eventRegistrationRepository.GetByCancelTokenAsync(request.Token, cancellationToken);
        if (registration is null)
        {
            return Result.Failure(InvalidTokenError);
        }

        if (registration.Status == EventRegistrationStatus.Cancelled)
        {
            return Result.Success();
        }

        var eventSchedule = await eventScheduleRepository.GetByContentItemIdAsync(registration.ContentItemId, cancellationToken);
        if (eventSchedule is null)
        {
            return Result.Failure(InvalidTokenError);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (now >= eventSchedule.StartsAtUtc)
        {
            return Result.Failure(AlreadyStartedError);
        }

        var cancelResult = registration.Cancel(EventRegistrationCancelledBy.Participant, now);
        if (cancelResult.IsFailure)
        {
            return Result.Failure(cancelResult.Error);
        }

        if (cancelResult.Value is EventRegistrationCancelOutcome.ReleasedNoSlot or EventRegistrationCancelOutcome.AlreadyCancelled)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        return await capacityRetryExecutor.ExecuteAsync(
            eventSchedule,
            () =>
            {
                if (cancelResult.Value == EventRegistrationCancelOutcome.ReleasedConfirmedSlot)
                {
                    eventSchedule.ReleaseConfirmedSlot();
                }
                else
                {
                    eventSchedule.ReleaseWaitlistSlot();
                }

                return Result.Success();
            },
            cancellationToken);
    }
}
