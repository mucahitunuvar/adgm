using GenclikMerkezi.Modules.Website.Application.Abstractions;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeMediaUsageChecker : IMediaUsageChecker
{
    public IReadOnlyList<MediaUsage> UsagesToReturn { get; set; } = [];

    public Task<IReadOnlyList<MediaUsage>> GetUsagesAsync(Guid mediaAssetId, CancellationToken cancellationToken = default) =>
        Task.FromResult(UsagesToReturn);
}
