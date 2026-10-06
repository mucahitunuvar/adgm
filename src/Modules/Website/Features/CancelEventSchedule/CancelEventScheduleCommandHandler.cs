using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.CancelEventSchedule;

// ADR-024 §11.1 (Faz 4 Görev 1): only the status transition and a no-op notifier call in this Görev -
// the real fan-out to Applied/Confirmed/Waitlisted registrations (IEventCancellationNotifier's real
// implementation) is Görev 4's job, once EventRegistration exists.
public sealed class CancelEventScheduleCommandHandler(
    IEventScheduleRepository eventScheduleRepository,
    IEventCancellationNotifier eventCancellationNotifier,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CancelEventScheduleCommand, Result>
{
    public async Task<Result> Handle(CancelEventScheduleCommand request, CancellationToken cancellationToken)
    {
        var eventSchedule = await eventScheduleRepository.GetByContentItemIdAsync(request.ContentItemId, cancellationToken);
        if (eventSchedule is null)
        {
            return Result.Failure(Error.NotFound("EventSchedule.NotFound", $"Content item '{request.ContentItemId}' has no event schedule."));
        }

        if (!request.RowVersion.SequenceEqual(eventSchedule.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "EventSchedule.ConcurrencyConflict", "The event schedule was changed by someone else. Reload and try again."));
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var cancelResult = eventSchedule.Cancel(request.Reason, currentUserContext.UserId!.Value, now);
        if (cancelResult.IsFailure)
        {
            return cancelResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        await eventCancellationNotifier.NotifyCancellationAsync(eventSchedule, cancellationToken);

        return Result.Success();
    }
}
