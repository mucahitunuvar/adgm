using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetEventScheduleByContentItemId;

public sealed class GetEventScheduleByContentItemIdQueryHandler(
    IContentItemRepository contentItemRepository, IEventScheduleRepository eventScheduleRepository)
    : IRequestHandler<GetEventScheduleByContentItemIdQuery, Result<EventScheduleDetailResponse>>
{
    public async Task<Result<EventScheduleDetailResponse>> Handle(
        GetEventScheduleByContentItemIdQuery request, CancellationToken cancellationToken)
    {
        var contentItem = await contentItemRepository.GetByIdAsync(request.ContentItemId, cancellationToken);
        if (contentItem is null)
        {
            return Result.Failure<EventScheduleDetailResponse>(
                Error.NotFound("ContentItem.NotFound", $"Content item '{request.ContentItemId}' could not be found."));
        }

        var eventSchedule = await eventScheduleRepository.GetByContentItemIdAsync(request.ContentItemId, cancellationToken);
        if (eventSchedule is null)
        {
            return Result.Failure<EventScheduleDetailResponse>(
                Error.NotFound("EventSchedule.NotFound", $"Content item '{request.ContentItemId}' has no event schedule."));
        }

        var translations = eventSchedule.Translations
            .Select(t => new EventScheduleTranslationResponse(
                t.LanguageCode.Value, t.VenueName, t.VenueAddress, t.FeeInfo, t.Instructors, t.ProgramFlow, t.AccessibilityNote))
            .ToList();

        var response = new EventScheduleDetailResponse(
            eventSchedule.Id, eventSchedule.ContentItemId, eventSchedule.StartsAtUtc, eventSchedule.EndsAtUtc,
            eventSchedule.Format.ToString(), eventSchedule.OnlineLink, eventSchedule.Capacity, eventSchedule.RegistrationEnabled,
            eventSchedule.RegistrationOpensAtUtc, eventSchedule.RegistrationClosesAtUtc, eventSchedule.MinAge, eventSchedule.MaxAge,
            eventSchedule.AutoConfirm, eventSchedule.WaitlistEnabled, eventSchedule.IsCancelled, eventSchedule.CancelledAtUtc,
            eventSchedule.CancellationReason, eventSchedule.ConfirmedCount, eventSchedule.WaitlistedCount, eventSchedule.RowVersion,
            translations);

        return Result.Success(response);
    }
}
