using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.Events;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ConfirmEventRegistration;

// ADR-024 §11.2 (Faz 4 Görev 4): the admin "confirm" decision - covers both a plain Applied -> Confirmed
// approval and a waitlist promotion (Waitlisted -> Confirmed, "-1 Waitlisted, +1 Confirmed" in the same
// Unit of Work, via EventSchedule.ConfirmRegistration). wasWaitlisted is captured once, before the
// retry loop - registration is never reloaded between attempts, only eventSchedule is (mirrors
// CancelEventRegistrationCommandHandler's own cancelResult.Value capture outside its own loop).
public sealed class ConfirmEventRegistrationCommandHandler(
    IEventRegistrationRepository eventRegistrationRepository,
    IEventScheduleRepository eventScheduleRepository,
    IContentItemRepository contentItemRepository,
    EventCapacityConcurrencyRetryExecutor capacityRetryExecutor,
    EventRegistrationNotifier notifier,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider)
    : IRequestHandler<ConfirmEventRegistrationCommand, Result>
{
    private static readonly Error NotFoundError = Error.NotFound(
        "EventRegistration.NotFound", "This registration could not be found.");

    private static readonly Error ConcurrencyError = Error.Conflict(
        "EventRegistration.ConcurrencyConflict", "The registration was changed by someone else. Reload and try again.");

    public async Task<Result> Handle(ConfirmEventRegistrationCommand request, CancellationToken cancellationToken)
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

        var wasWaitlisted = registration.Status == EventRegistrationStatus.Waitlisted;
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var changedBy = currentUserContext.UserId!.Value.ToString();

        var result = await capacityRetryExecutor.ExecuteAsync(
            eventSchedule,
            () =>
            {
                var confirmResult = eventSchedule.ConfirmRegistration(wasWaitlisted);
                return confirmResult.IsFailure ? confirmResult : registration.AdminConfirm(changedBy, now);
            },
            cancellationToken);

        if (result.IsFailure)
        {
            return result;
        }

        registration.ConfirmCapacityDecisionCommitted();

        var contentItem = await contentItemRepository.GetByIdAsync(registration.ContentItemId, cancellationToken);
        var eventTitle = contentItem?.Translations.FirstOrDefault(t => t.LanguageCode == registration.LanguageCode)?.Title
            ?? contentItem?.Translations.FirstOrDefault()?.Title ?? string.Empty;

        await notifier.SendCapacityDecisionEmailAsync(registration, eventSchedule, eventTitle, cancellationToken);

        return Result.Success();
    }
}
