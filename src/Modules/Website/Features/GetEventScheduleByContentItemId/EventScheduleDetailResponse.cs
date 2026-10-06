namespace GenclikMerkezi.Modules.Website.Features.GetEventScheduleByContentItemId;

public sealed record EventScheduleDetailResponse(
    Guid Id,
    Guid ContentItemId,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    string Format,
    string? OnlineLink,
    int? Capacity,
    bool RegistrationEnabled,
    DateTime? RegistrationOpensAtUtc,
    DateTime? RegistrationClosesAtUtc,
    int? MinAge,
    int? MaxAge,
    bool AutoConfirm,
    bool WaitlistEnabled,
    bool IsCancelled,
    DateTime? CancelledAtUtc,
    string? CancellationReason,
    int ConfirmedCount,
    int WaitlistedCount,
    byte[] RowVersion,
    IReadOnlyList<EventScheduleTranslationResponse> Translations);
