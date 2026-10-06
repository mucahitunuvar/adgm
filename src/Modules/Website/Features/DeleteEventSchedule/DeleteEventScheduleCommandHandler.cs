using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeleteEventSchedule;

// ADR-024 §11.1/§11.2 (Faz 4 Görev 1): "yalnızca kayıt yoksa" - IEventRegistrationUsageChecker is a
// no-op until Görev 3 adds EventRegistration, so this never blocks anything yet; the guard is wired
// now so Görev 3 only has to replace the checker's implementation, not this handler.
public sealed class DeleteEventScheduleCommandHandler(
    IEventScheduleRepository eventScheduleRepository,
    IEventRegistrationUsageChecker eventRegistrationUsageChecker,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteEventScheduleCommand, Result>
{
    public async Task<Result> Handle(DeleteEventScheduleCommand request, CancellationToken cancellationToken)
    {
        var eventSchedule = await eventScheduleRepository.GetByContentItemIdAsync(request.ContentItemId, cancellationToken);
        if (eventSchedule is null)
        {
            return Result.Failure(Error.NotFound("EventSchedule.NotFound", $"Content item '{request.ContentItemId}' has no event schedule."));
        }

        var hasRegistrations = await eventRegistrationUsageChecker.HasRegistrationsAsync(request.ContentItemId, cancellationToken);
        if (hasRegistrations)
        {
            return Result.Failure(Error.Conflict("Event.HasRegistrations", "Cannot delete the event schedule: this event still has registrations."));
        }

        eventScheduleRepository.Remove(eventSchedule);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
