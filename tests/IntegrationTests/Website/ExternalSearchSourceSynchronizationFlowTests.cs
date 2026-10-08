using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.Search;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Infrastructure.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §10 (Faz 5 Görev 4). Exercises ExternalSearchSourceSynchronizer directly (same no-HTTP-
// endpoint, DI-scope pattern as SearchDocumentPersistenceFlowTests/Görev 1) against hand-rolled
// FakeExternalSearchSource instances instead of the real Employer adapter - the full-sync algorithm
// itself (upsert/delete/partial-failure/isolation/locking) does not depend on which source it runs for.
public class ExternalSearchSourceSynchronizationFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ExternalSearchSourceSynchronizationFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static LanguageCode Tr() => LanguageCode.Create("tr").Value;

    private static ExternalSearchDocument NewDocument(string sourceId, string title, DateTime publishedAtUtc) =>
        new(sourceId, "job", title, "Summary", $"/ilanlar/{sourceId}", $"{title} Summary", publishedAtUtc, IncludeInSitemap: true);

    [Fact]
    public async Task SyncAsync_UpsertsEveryDocument_AndRecordsSucceededState()
    {
        using var scope = _factory.Services.CreateScope();
        var synchronizer = scope.ServiceProvider.GetRequiredService<ExternalSearchSourceSynchronizer>();
        var searchDocumentRepository = scope.ServiceProvider.GetRequiredService<ISearchDocumentRepository>();
        var searchSourceStateRepository = scope.ServiceProvider.GetRequiredService<ISearchSourceStateRepository>();

        var sourceKey = $"fake-{Guid.NewGuid():N}";
        var source = new FakeExternalSearchSource(sourceKey)
        {
            Documents =
            [
                NewDocument("a", "Job A", DateTime.UtcNow),
                NewDocument("b", "Job B", DateTime.UtcNow),
            ],
        };

        var result = await synchronizer.SyncAsync(source);

        Assert.True(result.IsSuccess);
        Assert.NotNull(await searchDocumentRepository.GetAsync(sourceKey, "a", Tr()));
        Assert.NotNull(await searchDocumentRepository.GetAsync(sourceKey, "b", Tr()));
        Assert.Equal(2, await searchDocumentRepository.CountBySourceAsync(sourceKey));

        var state = await searchSourceStateRepository.GetAsync(sourceKey);
        Assert.NotNull(state);
        Assert.NotNull(state!.LastStartedAtUtc);
        Assert.NotNull(state.LastSucceededAtUtc);
        Assert.Null(state.LastError);
        Assert.Equal(2, state.DocumentCount);
    }

    [Fact]
    public async Task SyncAsync_WhenRunAgainWithAChangedSet_UpdatesAndDeletesMissingDocuments()
    {
        using var scope = _factory.Services.CreateScope();
        var synchronizer = scope.ServiceProvider.GetRequiredService<ExternalSearchSourceSynchronizer>();
        var searchDocumentRepository = scope.ServiceProvider.GetRequiredService<ISearchDocumentRepository>();

        var sourceKey = $"fake-{Guid.NewGuid():N}";
        var source = new FakeExternalSearchSource(sourceKey)
        {
            Documents =
            [
                NewDocument("a", "Job A", DateTime.UtcNow),
                NewDocument("b", "Job B", DateTime.UtcNow),
            ],
        };
        await synchronizer.SyncAsync(source);

        source.Documents = [NewDocument("a", "Job A - Updated", DateTime.UtcNow), NewDocument("c", "Job C", DateTime.UtcNow)];
        await synchronizer.SyncAsync(source);

        var a = await searchDocumentRepository.GetAsync(sourceKey, "a", Tr());
        Assert.NotNull(a);
        Assert.Equal("Job A - Updated", a!.Title);
        Assert.Null(await searchDocumentRepository.GetAsync(sourceKey, "b", Tr()));
        Assert.NotNull(await searchDocumentRepository.GetAsync(sourceKey, "c", Tr()));
        Assert.Equal(2, await searchDocumentRepository.CountBySourceAsync(sourceKey));
    }

    [Fact]
    public async Task SyncAsync_WhenAPageThrows_DeletesNothing_AndRecordsTheFailure()
    {
        using var scope = _factory.Services.CreateScope();
        var synchronizer = scope.ServiceProvider.GetRequiredService<ExternalSearchSourceSynchronizer>();
        var searchDocumentRepository = scope.ServiceProvider.GetRequiredService<ISearchDocumentRepository>();
        var searchSourceStateRepository = scope.ServiceProvider.GetRequiredService<ISearchSourceStateRepository>();

        var sourceKey = $"fake-{Guid.NewGuid():N}";
        var source = new FakeExternalSearchSource(sourceKey)
        {
            Documents = [NewDocument("a", "Job A", DateTime.UtcNow), NewDocument("b", "Job B", DateTime.UtcNow)],
        };
        await synchronizer.SyncAsync(source);

        // If this run succeeded, "b" would be dropped (it is no longer in Documents) - but page 1
        // throws first, so DeleteUnseenBySourceAsync must never run.
        source.Documents = [NewDocument("a", "Job A", DateTime.UtcNow)];
        source.FailingPages = [1];
        var result = await synchronizer.SyncAsync(source);

        Assert.True(result.IsSuccess); // the failure is recorded on SearchSourceState, not surfaced as a Result failure.
        Assert.NotNull(await searchDocumentRepository.GetAsync(sourceKey, "a", Tr()));
        Assert.NotNull(await searchDocumentRepository.GetAsync(sourceKey, "b", Tr()));

        var state = await searchSourceStateRepository.GetAsync(sourceKey);
        Assert.NotNull(state!.LastError);
        Assert.Contains("Simulated failure on page 1", state.LastError!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SyncAsync_TwoSources_OneFailingOneSucceeding_AreIsolatedFromEachOther()
    {
        using var scope = _factory.Services.CreateScope();
        var synchronizer = scope.ServiceProvider.GetRequiredService<ExternalSearchSourceSynchronizer>();
        var searchDocumentRepository = scope.ServiceProvider.GetRequiredService<ISearchDocumentRepository>();
        var searchSourceStateRepository = scope.ServiceProvider.GetRequiredService<ISearchSourceStateRepository>();

        var goodSourceKey = $"fake-good-{Guid.NewGuid():N}";
        var badSourceKey = $"fake-bad-{Guid.NewGuid():N}";
        var goodSource = new FakeExternalSearchSource(goodSourceKey) { Documents = [NewDocument("a", "Job A", DateTime.UtcNow)] };
        var badSource = new FakeExternalSearchSource(badSourceKey) { FailingPages = [1] };

        var badResult = await synchronizer.SyncAsync(badSource);
        var goodResult = await synchronizer.SyncAsync(goodSource);

        Assert.True(badResult.IsSuccess);
        Assert.True(goodResult.IsSuccess);
        Assert.NotNull((await searchSourceStateRepository.GetAsync(badSourceKey))!.LastError);
        Assert.Null((await searchSourceStateRepository.GetAsync(goodSourceKey))!.LastError);
        Assert.NotNull(await searchDocumentRepository.GetAsync(goodSourceKey, "a", Tr()));
    }

    [Fact]
    public async Task SyncAsync_WhenAlreadyRunningForTheSameSource_ReturnsConflict()
    {
        using var scope = _factory.Services.CreateScope();
        var synchronizer = scope.ServiceProvider.GetRequiredService<ExternalSearchSourceSynchronizer>();
        var coordinator = scope.ServiceProvider.GetRequiredService<SearchSourceSyncCoordinator>();

        var sourceKey = $"fake-{Guid.NewGuid():N}";
        var source = new FakeExternalSearchSource(sourceKey) { Documents = [NewDocument("a", "Job A", DateTime.UtcNow)] };

        Assert.True(coordinator.TryEnter(sourceKey));
        try
        {
            var result = await synchronizer.SyncAsync(source);

            Assert.True(result.IsFailure);
            Assert.Equal("SearchSource.AlreadyRunning", result.Error.Code);
        }
        finally
        {
            coordinator.Exit(sourceKey);
        }
    }

    [Fact]
    public async Task SyncAsync_LoopsThroughEveryPage_UntilTheSourceIsExhausted()
    {
        using var scope = _factory.Services.CreateScope();
        var synchronizer = scope.ServiceProvider.GetRequiredService<ExternalSearchSourceSynchronizer>();
        var searchDocumentRepository = scope.ServiceProvider.GetRequiredService<ISearchDocumentRepository>();

        var sourceKey = $"fake-{Guid.NewGuid():N}";
        var now = DateTime.UtcNow;
        var source = new FakeExternalSearchSource(sourceKey)
        {
            Documents = Enumerable.Range(0, 450).Select(i => NewDocument($"id-{i}", $"Job {i}", now.AddMinutes(-i))).ToList(),
        };

        await synchronizer.SyncAsync(source);

        Assert.Equal(450, await searchDocumentRepository.CountBySourceAsync(sourceKey));
    }

    // ADR-024 §10: "Kaynak kayıtlı değilse... job boş çalışır, hata vermez" - CustomWebApplicationFactory
    // boots the real Program, so IExternalSearchSource already has one real registration
    // (EmployerJobSearchSource, PublicJobListingsEnabled off by default); this proves the job's own
    // IServiceScopeFactory/DI wiring runs end to end without throwing, on top of
    // IExternalSearchSource_WithNoneRegistered_ResolvesToAnEmptyCollection (Görev 1, a different
    // factory with no Host adapter wired up at all).
    [Fact]
    public async Task SyncExternalSearchSourcesJob_RunsEndToEnd_WithoutThrowing()
    {
        using var scope = _factory.Services.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<SyncExternalSearchSourcesJob>();

        await job.ExecuteAsync();
    }
}
