using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.Events;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.RejectEventRegistration;

// ADR-024 §11.2 (Faz 4 Görev 4): the admin "reject" decision - valid from Applied or Waitlisted (a
// Confirmed registration is withdrawn via admin Cancel instead). Reason is never persisted or shown to
// the participant (§1 "reason opsiyonel, kişiye iletilmez"). Reject runs once, outside the retry loop,
// exactly like CancelEventRegistrationCommandHandler's own Cancel call - only the counter release (when
// the outcome calls for one) goes through EventCapacityConcurrencyRetryExecutor.
public sealed class RejectEventRegistrationCommandHandler(
    IEventRegistrationRepository eventRegistrationRepository,
    IEventScheduleRepository eventScheduleRepository,
    IContentItemRepository contentItemRepository,
    EventCapacityConcurrencyRetryExecutor capacityRetryExecutor,
    EventRegistrationNotifier notifier,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<RejectEventRegistrationCommand, Result>
{
    private static readonly Error NotFoundError = Error.NotFound(
        "EventRegistration.NotFound", "This registration could not be found.");

    private static readonly Error ConcurrencyError = Error.Conflict(
        "EventRegistration.ConcurrencyConflict", "The registration was changed by someone else. Reload and try again.");

    public async Task<Result> Handle(RejectEventRegistrationCommand request, CancellationToken cancellationToken)
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

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var changedBy = currentUserContext.UserId!.Value.ToString();

        var rejectResult = registration.Reject(changedBy, now);
        if (rejectResult.IsFailure)
        {
            return Result.Failure(rejectResult.Error);
        }

        if (rejectResult.Value == EventRegistrationCancelOutcome.ReleasedWaitlistSlot)
        {
            var releaseResult = await capacityRetryExecutor.ExecuteAsync(
                eventSchedule,
                () =>
                {
                    eventSchedule.ReleaseWaitlistSlot();
                    return Result.Success();
                },
                cancellationToken);

            if (releaseResult.IsFailure)
            {
                return releaseResult;
            }
        }
        else
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var contentItem = await contentItemRepository.GetByIdAsync(registration.ContentItemId, cancellationToken);
        var eventTitle = contentItem?.Translations.FirstOrDefault(t => t.LanguageCode == registration.LanguageCode)?.Title
            ?? contentItem?.Translations.FirstOrDefault()?.Title ?? string.Empty;

        await notifier.SendRejectionNoticeAsync(registration, eventTitle, cancellationToken);

        return Result.Success();
    }
}
