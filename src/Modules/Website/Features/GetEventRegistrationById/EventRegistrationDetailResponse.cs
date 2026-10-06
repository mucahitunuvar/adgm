using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Features.GetEventRegistrationById;

public sealed record EventRegistrationDetailResponse(
    Guid Id,
    Guid EventScheduleId,
    Guid ContentItemId,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    Guid? UserId,
    string LanguageCode,
    EventRegistrationStatus Status,
    string AcceptedPrivacyNoticeKey,
    int AcceptedPrivacyNoticeVersion,
    DateTime CreatedAtUtc,
    DateTime? VerifiedAtUtc,
    DateTime? StatusChangedAtUtc,
    DateTime? WaitlistedAtUtc,
    DateTime? CancelledAtUtc,
    string? CancelledBy,
    DateTime? AnonymizedAtUtc,
    byte[] RowVersion,
    IReadOnlyList<EventRegistrationStatusHistoryEntryResponse> StatusHistory);
