using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeEventScheduleRepository : IEventScheduleRepository
{
    private readonly List<EventSchedule> _eventSchedules = [];

    // ExistsForContentTypeIdAsync's real implementation joins through ContentItem, which this fake
    // (deliberately, like FakeContentItemRepository.FullPathExistsResult) does not model - callers that
    // need it set this directly instead.
    public bool ExistsForContentTypeResult { get; set; }

    public void Seed(EventSchedule eventSchedule) => _eventSchedules.Add(eventSchedule);

    public Task<EventSchedule?> GetByContentItemIdAsync(Guid contentItemId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_eventSchedules.FirstOrDefault(es => es.ContentItemId == contentItemId));

    public Task<bool> ExistsForContentTypeIdAsync(Guid contentTypeId, CancellationToken cancellationToken = default) =>
        Task.FromResult(ExistsForContentTypeResult);

    // SearchPublicAsync's real implementation joins through ContentItem/ContentType (title, path,
    // cover image, type flags), which this fake has no access to - GetPublicEventsQueryHandler is
    // covered by integration tests against the real repository instead.
    public Task<PagedResult<PublicEventListItemCandidate>> SearchPublicAsync(
        Guid? contentTypeId, EventFormat? format, DateTime? from, DateTime? to, EventTimeWindow window, DateTime now,
        LanguageCode languageCode, PagedRequest pagedRequest, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not modeled by this fake - covered by integration tests against the real repository.");

    public Task<EventRegistrationStateInputs?> GetRegistrationStateInputsByContentItemIdAsync(
        Guid contentItemId, CancellationToken cancellationToken = default)
    {
        var eventSchedule = _eventSchedules.FirstOrDefault(es => es.ContentItemId == contentItemId);
        return Task.FromResult(eventSchedule is null ? null : ToInputs(eventSchedule));
    }

    public Task<IReadOnlyDictionary<Guid, EventRegistrationStateInputs>> GetRegistrationStateInputsByContentItemIdsAsync(
        IReadOnlyList<Guid> contentItemIds, CancellationToken cancellationToken = default)
    {
        IReadOnlyDictionary<Guid, EventRegistrationStateInputs> result = _eventSchedules
            .Where(es => contentItemIds.Contains(es.ContentItemId))
            .ToDictionary(es => es.ContentItemId, ToInputs);
        return Task.FromResult(result);
    }

    public void Add(EventSchedule eventSchedule) => _eventSchedules.Add(eventSchedule);

    public void Remove(EventSchedule eventSchedule) => _eventSchedules.Remove(eventSchedule);

    // Nothing to reload against - this fake has no separate backing store distinct from the seeded
    // instance itself (see EventCapacityConcurrencyRetryExecutorTests for how conflict/retry is
    // exercised instead, against a FakeUnitOfWork that throws DbUpdateConcurrencyException once).
    public Task ReloadAsync(EventSchedule eventSchedule, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<IReadOnlyList<Guid>> GetIdsEndedBeforeAsync(DateTime endedBeforeUtc, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Guid> ids = _eventSchedules.Where(es => es.EndsAtUtc <= endedBeforeUtc).Select(es => es.Id).ToList();
        return Task.FromResult(ids);
    }

    private static EventRegistrationStateInputs ToInputs(EventSchedule es) =>
        new(es.IsCancelled, es.RegistrationEnabled, es.RegistrationOpensAtUtc, es.RegistrationClosesAtUtc, es.StartsAtUtc, es.Capacity,
            es.ConfirmedCount, es.WaitlistEnabled);
}
