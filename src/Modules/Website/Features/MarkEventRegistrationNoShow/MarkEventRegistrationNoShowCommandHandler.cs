using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.MarkEventRegistrationNoShow;

// §1 "QR giriş, yoklama ekranı... yoktur... Attended/NoShow yalnızca admin listesinde elle
// işaretlenir" - only valid from Confirmed, only once the event has started. Never touches
// EventSchedule's counters (already counted at Confirmed) and sends no email.
public sealed class MarkEventRegistrationNoShowCommandHandler(
    IEventRegistrationRepository eventRegistrationRepository,
    IEventScheduleRepository eventScheduleRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<MarkEventRegistrationNoShowCommand, Result>
{
    private static readonly Error NotFoundError = Error.NotFound(
        "EventRegistration.NotFound", "This registration could not be found.");

    private static readonly Error ConcurrencyError = Error.Conflict(
        "EventRegistration.ConcurrencyConflict", "The registration was changed by someone else. Reload and try again.");

    private static readonly Error NotStartedYetError = Error.Conflict(
        "Event.NotStartedYet", "Attendance cannot be recorded before the event has started.");

    public async Task<Result> Handle(MarkEventRegistrationNoShowCommand request, CancellationToken cancellationToken)
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
        if (now < eventSchedule.StartsAtUtc)
        {
            return Result.Failure(NotStartedYetError);
        }

        var markResult = registration.MarkNoShow(currentUserContext.UserId!.Value.ToString(), now);
        if (markResult.IsFailure)
        {
            return markResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
