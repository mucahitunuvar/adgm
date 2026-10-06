using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IEventScheduleRepository
{
    Task<EventSchedule?> GetByContentItemIdAsync(Guid contentItemId, CancellationToken cancellationToken = default);

    // Cross-aggregate check for UpdateContentTypeCommandHandler's "SupportsEvent cannot be turned off
    // while a content item of this type still has a calendar" guard (ADR-024 §11.1 / Faz 4 Görev 1) -
    // EventSchedule has no direct ContentTypeId of its own, so this joins through ContentItemId.
    Task<bool> ExistsForContentTypeIdAsync(Guid contentTypeId, CancellationToken cancellationToken = default);

    // ADR-024 §17 (Faz 4 Görev 2): GetPublicEvents' cached candidate page - inner join (an item with no
    // schedule can never appear here), restricted to active SupportsEvent content types, visible
    // ContentItems, translated in languageCode. contentTypeId narrows to one type when given (the
    // caller has already validated it is active and SupportsEvent); null means every active
    // SupportsEvent type. Sort/time-filter follows window ("past sıralaması azalan").
    Task<PagedResult<PublicEventListItemCandidate>> SearchPublicAsync(
        Guid? contentTypeId,
        EventFormat? format,
        DateTime? from,
        DateTime? to,
        EventTimeWindow window,
        DateTime now,
        LanguageCode languageCode,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default);

    // ADR-024 §17 (Faz 4 Görev 2): the uncached registration-state inputs for a single ContentItemId -
    // used by the public content detail endpoint, run fresh every request after the cached response is
    // read. Null when the content item has no schedule.
    Task<EventRegistrationStateInputs?> GetRegistrationStateInputsByContentItemIdAsync(
        Guid contentItemId, CancellationToken cancellationToken = default);

    // Same as above, batched for a whole page of ids - used by GetPublicEvents after SearchPublicAsync
    // returns its (cached) page of candidates.
    Task<IReadOnlyDictionary<Guid, EventRegistrationStateInputs>> GetRegistrationStateInputsByContentItemIdsAsync(
        IReadOnlyList<Guid> contentItemIds, CancellationToken cancellationToken = default);

    void Add(EventSchedule eventSchedule);

    void Remove(EventSchedule eventSchedule);

    // ADR-024 §11.2 (Faz 4 Görev 3): discards eventSchedule's in-memory, not-yet-saved changes
    // (ReserveCapacity/ReleaseConfirmedSlot/ReleaseWaitlistSlot, bumped RowVersion included) and
    // refreshes it from the database - used by EventCapacityConcurrencyRetryExecutor between retry
    // attempts after a RowVersion conflict, so the next attempt's capacity decision is made against
    // the row's current ConfirmedCount/WaitlistedCount/RowVersion, not the stale values the failed
    // attempt started from.
    Task ReloadAsync(EventSchedule eventSchedule, CancellationToken cancellationToken = default);
}
