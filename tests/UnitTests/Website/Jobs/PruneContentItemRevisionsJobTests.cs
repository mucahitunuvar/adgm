using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Infrastructure.Jobs;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.UnitTests.Website.Jobs;

// ADR-024 §4 (Faz 5 Görev 7). Same "job resolves its own IServiceScopeFactory from a ServiceCollection,
// Hangfire runtime never involved" pattern PermanentlyDeleteExpiredTrashJobTests already uses.
public class PruneContentItemRevisionsJobTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeContentItemRevisionRepository _repository = new();

    private PruneContentItemRevisionsJob CreateJob()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IContentItemRevisionRepository>(_repository);
        var serviceProvider = services.BuildServiceProvider();

        return new PruneContentItemRevisionsJob(serviceProvider.GetRequiredService<IServiceScopeFactory>());
    }

    private void SeedRevisions(Guid contentItemId, int count, int? publishedSnapshotRevisionNumber = null)
    {
        for (var number = 1; number <= count; number++)
        {
            var kind = number == publishedSnapshotRevisionNumber ? ContentItemRevisionKind.Published : ContentItemRevisionKind.Edited;
            _repository.Seed(ContentItemRevision.Create(contentItemId, number, Now, UserId, kind, [], $"hash-{number}", "{}"));
        }
    }

    [Fact]
    public async Task ExecuteAsync_ContentItemWithFiftyOrFewerRevisions_DeletesNothing()
    {
        var contentItemId = Guid.NewGuid();
        SeedRevisions(contentItemId, 50);

        await CreateJob().ExecuteAsync(CancellationToken.None);

        var remaining = await _repository.GetRetentionRowsAsync(contentItemId, CancellationToken.None);
        Assert.Equal(50, remaining.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ContentItemWithSixtyRevisions_KeepsOnlyTheNewestFifty()
    {
        var contentItemId = Guid.NewGuid();
        SeedRevisions(contentItemId, 60);

        await CreateJob().ExecuteAsync(CancellationToken.None);

        var remaining = await _repository.GetRetentionRowsAsync(contentItemId, CancellationToken.None);
        Assert.Equal(50, remaining.Count);
        Assert.Equal(Enumerable.Range(11, 50), remaining.Select(r => r.RevisionNumber).OrderBy(n => n));
    }

    [Fact]
    public async Task ExecuteAsync_PublishedSnapshotOutsideTheKeptWindow_IsPreservedAnyway()
    {
        var contentItemId = Guid.NewGuid();
        // Revision #5 is the published snapshot, well outside the newest-50 window (11..60).
        SeedRevisions(contentItemId, 60, publishedSnapshotRevisionNumber: 5);

        await CreateJob().ExecuteAsync(CancellationToken.None);

        var remaining = await _repository.GetRetentionRowsAsync(contentItemId, CancellationToken.None);
        Assert.Equal(51, remaining.Count);
        Assert.Contains(remaining, r => r.RevisionNumber == 5 && r.IsPublishedSnapshot);
        Assert.DoesNotContain(remaining, r => r.RevisionNumber == 6);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleContentItems_EachPrunedIndependently()
    {
        var firstItemId = Guid.NewGuid();
        var secondItemId = Guid.NewGuid();
        SeedRevisions(firstItemId, 60);
        SeedRevisions(secondItemId, 10);

        await CreateJob().ExecuteAsync(CancellationToken.None);

        Assert.Equal(50, (await _repository.GetRetentionRowsAsync(firstItemId, CancellationToken.None)).Count);
        Assert.Equal(10, (await _repository.GetRetentionRowsAsync(secondItemId, CancellationToken.None)).Count);
    }
}
