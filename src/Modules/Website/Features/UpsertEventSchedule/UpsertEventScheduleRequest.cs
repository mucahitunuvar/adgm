namespace GenclikMerkezi.Modules.Website.Features.UpsertEventSchedule;

public sealed record UpsertEventScheduleRequest(
    byte[]? RowVersion,
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
    IReadOnlyList<UpsertEventScheduleTranslationInput> Translations);
