using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetEventRegistrationById;

public sealed class GetEventRegistrationByIdQueryHandler(IEventRegistrationRepository eventRegistrationRepository)
    : IRequestHandler<GetEventRegistrationByIdQuery, Result<EventRegistrationDetailResponse>>
{
    private static readonly Error NotFoundError = Error.NotFound(
        "EventRegistration.NotFound", "This registration could not be found.");

    public async Task<Result<EventRegistrationDetailResponse>> Handle(
        GetEventRegistrationByIdQuery request, CancellationToken cancellationToken)
    {
        var registration = await eventRegistrationRepository.GetByIdAsync(request.Id, cancellationToken);
        if (registration is null || registration.ContentItemId != request.ContentItemId)
        {
            return Result.Failure<EventRegistrationDetailResponse>(NotFoundError);
        }

        var statusHistory = registration.StatusHistory
            .OrderBy(h => h.OccurredAtUtc)
            .Select(h => new EventRegistrationStatusHistoryEntryResponse(
                h.PreviousStatus?.ToString(), h.NewStatus.ToString(), h.ChangedBy, h.OccurredAtUtc))
            .ToList();

        var response = new EventRegistrationDetailResponse(
            registration.Id,
            registration.EventScheduleId,
            registration.ContentItemId,
            registration.FirstName,
            registration.LastName,
            registration.Email,
            registration.Phone,
            registration.UserId,
            registration.LanguageCode.Value,
            registration.Status,
            registration.AcceptedPrivacyNoticeKey.Value,
            registration.AcceptedPrivacyNoticeVersion,
            registration.CreatedAtUtc,
            registration.VerifiedAtUtc,
            registration.StatusChangedAtUtc,
            registration.WaitlistedAtUtc,
            registration.CancelledAtUtc,
            registration.CancelledBy?.ToString(),
            registration.AnonymizedAtUtc,
            registration.RowVersion,
            statusHistory);

        return Result.Success(response);
    }
}
