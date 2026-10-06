using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetEventRegistrations;

public sealed class GetEventRegistrationsQueryHandler(
    IEventRegistrationRepository eventRegistrationRepository, IEventScheduleRepository eventScheduleRepository)
    : IRequestHandler<GetEventRegistrationsQuery, Result<GetEventRegistrationsResponse>>
{
    private static readonly Error NotFoundError = Error.NotFound(
        "EventSchedule.NotFound", "This content item has no event schedule.");

    public async Task<Result<GetEventRegistrationsResponse>> Handle(
        GetEventRegistrationsQuery request, CancellationToken cancellationToken)
    {
        var eventSchedule = await eventScheduleRepository.GetByContentItemIdAsync(request.ContentItemId, cancellationToken);
        if (eventSchedule is null)
        {
            return Result.Failure<GetEventRegistrationsResponse>(NotFoundError);
        }

        var paged = await eventRegistrationRepository.SearchAsync(
            request.ContentItemId, request.Status, request.Search, request, cancellationToken);
        var statusCounts = await eventRegistrationRepository.GetStatusCountsAsync(request.ContentItemId, cancellationToken);

        var counters = new EventRegistrationCountersResponse(
            statusCounts.GetValueOrDefault(EventRegistrationStatus.Confirmed),
            statusCounts.GetValueOrDefault(EventRegistrationStatus.Applied),
            statusCounts.GetValueOrDefault(EventRegistrationStatus.Waitlisted),
            eventSchedule.Capacity is null ? null : Math.Max(0, eventSchedule.Capacity.Value - eventSchedule.ConfirmedCount));

        var items = paged.Items
            .Select(i => new EventRegistrationSummaryResponse(
                i.Id, i.FirstName, i.LastName, i.Email, i.Phone, i.Status, i.CreatedAtUtc, i.VerifiedAtUtc, i.WaitlistedAtUtc,
                i.RowVersion))
            .ToList();

        var response = new GetEventRegistrationsResponse(
            counters, new PagedResult<EventRegistrationSummaryResponse>(items, paged.TotalCount, paged.Page, paged.PageSize));

        return Result.Success(response);
    }
}
