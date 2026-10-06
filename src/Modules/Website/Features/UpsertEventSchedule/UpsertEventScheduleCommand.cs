using GenclikMerkezi.Modules.Website.Features.GetEventScheduleByContentItemId;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UpsertEventSchedule;

// RowVersion is empty/absent when creating (no EventSchedule exists yet for this content item) and
// required - and compared - when updating an existing one (ADR-024 §11.1, same optimistic-concurrency
// contract as every other Website aggregate).
public sealed record UpsertEventScheduleCommand(
    Guid ContentItemId,
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
    IReadOnlyList<UpsertEventScheduleTranslationInput> Translations) : IRequest<Result<EventScheduleDetailResponse>>;
