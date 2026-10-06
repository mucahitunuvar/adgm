using GenclikMerkezi.Modules.Website.Application.Abstractions;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeEventCancellationBatchScheduler : IEventCancellationBatchScheduler
{
    public List<(Guid ContentItemId, int Skip)> ScheduledBatches { get; } = [];

    public void ScheduleNextBatch(Guid contentItemId, int skip) => ScheduledBatches.Add((contentItemId, skip));
}
