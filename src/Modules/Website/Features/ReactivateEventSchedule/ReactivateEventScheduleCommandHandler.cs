using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.ReactivateEventSchedule;

public sealed class ReactivateEventScheduleCommandHandler(
    IEventScheduleRepository eventScheduleRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ReactivateEventScheduleCommand, Result>
{
    public async Task<Result> Handle(ReactivateEventScheduleCommand request, CancellationToken cancellationToken)
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

        var reactivateResult = eventSchedule.Reactivate(currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (reactivateResult.IsFailure)
        {
            return reactivateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
