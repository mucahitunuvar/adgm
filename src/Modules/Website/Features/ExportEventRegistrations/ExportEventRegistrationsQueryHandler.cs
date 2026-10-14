using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Parsing;
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
        EventRegistrationStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!EnumParsing.TryParseDefined<EventRegistrationStatus>(request.Status, out var parsedStatus))
            {
                return Result.Failure<IReadOnlyList<EventRegistrationExportItem>>(Error.Validation(
                    "EventRegistration.InvalidStatusFilter", $"'{request.Status}' is not a recognized event registration status."));
            }

            status = parsedStatus;
        }

        var eventSchedule = await eventScheduleRepository.GetByContentItemIdAsync(request.ContentItemId, cancellationToken);
        if (eventSchedule is null)
        {
            return Result.Failure<IReadOnlyList<EventRegistrationExportItem>>(NotFoundError);
        }

        var items = await eventRegistrationRepository.GetForExportAsync(request.ContentItemId, status, cancellationToken);
        return Result.Success(items);
    }
}
