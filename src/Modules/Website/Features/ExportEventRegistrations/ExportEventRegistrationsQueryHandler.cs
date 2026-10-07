using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ExportEventRegistrations;

public sealed class ExportEventRegistrationsQueryHandler(
    IEventRegistrationRepository eventRegistrationRepository, IEventScheduleRepository eventScheduleRepository)
    : IRequestHandler<ExportEventRegistrationsQuery, Result<IReadOnlyList<EventRegistrationExportItem>>>
{
    private static readonly Error NotFoundError = Error.NotFound(
        "EventSchedule.NotFound", "This content item has no event schedule.");

    public async Task<Result<IReadOnlyList<EventRegistrationExportItem>>> Handle(
        ExportEventRegistrationsQuery request, CancellationToken cancellationToken)
    {
        var eventSchedule = await eventScheduleRepository.GetByContentItemIdAsync(request.ContentItemId, cancellationToken);
        if (eventSchedule is null)
        {
            return Result.Failure<IReadOnlyList<EventRegistrationExportItem>>(NotFoundError);
        }

        var items = await eventRegistrationRepository.GetForExportAsync(request.ContentItemId, request.Status, cancellationToken);
        return Result.Success(items);
    }
}
