using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

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

    public void Add(EventSchedule eventSchedule) => _eventSchedules.Add(eventSchedule);

    public void Remove(EventSchedule eventSchedule) => _eventSchedules.Remove(eventSchedule);
}
