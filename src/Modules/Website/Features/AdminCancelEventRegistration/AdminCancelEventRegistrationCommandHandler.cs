using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.Events;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.AdminCancelEventRegistration;

// ADR-024 §11.2 (Faz 4 Görev 4): the admin-initiated cancellation (CancelledBy = Admin) - otherwise the
// same shape as CancelEventRegistrationCommandHandler's own participant-facing flow (Cancel() runs
// once, outside the retry loop; only the counter release goes through
// EventCapacityConcurrencyRetryExecutor), identified by RowVersion rather than a cancellation token,
// gated by Website.Submissions.Manage rather than being anonymous/token-based, and - unlike the
// participant's own cancellation, which needs no notice - followed by an informational email, since
// the participant did not trigger this themselves.
public sealed class AdminCancelEventRegistrationCommandHandler(
    IEventRegistrationRepository eventRegistrationRepository,
    IEventScheduleRepository eventScheduleRepository,
    IContentItemRepository contentItemRepository,
    EventCapacityConcurrencyRetryExecutor capacityRetryExecutor,
    EventRegistrationNotifier notifier,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<AdminCancelEventRegistrationCommand, Result>
{
    private static readonly Error NotFoundError = Error.NotFound(
        "EventRegistration.NotFound", "This registration could not be found.");

    private static readonly Error ConcurrencyError = Error.Conflict(
        "EventRegistration.ConcurrencyConflict", "The registration was changed by someone else. Reload and try again.");

    public async Task<Result> Handle(AdminCancelEventRegistrationCommand request, CancellationToken cancellationToken)
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
        var cancelResult = registration.Cancel(EventRegistrationCancelledBy.Admin, now);
        if (cancelResult.IsFailure)
        {
            return Result.Failure(cancelResult.Error);
        }

        if (cancelResult.Value is EventRegistrationCancelOutcome.ReleasedNoSlot or EventRegistrationCancelOutcome.AlreadyCancelled)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var releaseResult = await capacityRetryExecutor.ExecuteAsync(
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

            if (releaseResult.IsFailure)
            {
                return releaseResult;
            }
        }

        if (cancelResult.Value != EventRegistrationCancelOutcome.AlreadyCancelled)
        {
            var contentItem = await contentItemRepository.GetByIdAsync(registration.ContentItemId, cancellationToken);
            var eventTitle = contentItem?.Translations.FirstOrDefault(t => t.LanguageCode == registration.LanguageCode)?.Title
                ?? contentItem?.Translations.FirstOrDefault()?.Title ?? string.Empty;

            await notifier.SendAdminCancellationNoticeAsync(registration, eventTitle, cancellationToken);
        }

        return Result.Success();
    }
}
