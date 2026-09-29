using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Infrastructure.Jobs;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.UnitTests.Website.Jobs;

// Hangfire runtime devreye girmez - Support modülünün CloseOverdueSupportTicketsJobTests deseniyle
// aynı: job kendi IServiceScopeFactory'sini bir ServiceCollection'dan çözer, bu yüzden sahte repository
// buraya kaydedilir ve ExecuteAsync doğrudan çağrılır.
public class CleanupStaleNotFoundLogsJobTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;

    private readonly FakeNotFoundLogRepository _notFoundLogRepository = new();

    private CleanupStaleNotFoundLogsJob CreateJob()
    {
        var services = new ServiceCollection();
        services.AddSingleton<INotFoundLogRepository>(_notFoundLogRepository);
        var serviceProvider = services.BuildServiceProvider();

        return new CleanupStaleNotFoundLogsJob(serviceProvider.GetRequiredService<IServiceScopeFactory>(), TimeProvider.System);
    }

    private NotFoundLog Seed(string path, DateTime lastSeenAtUtc, int hitCount)
    {
        var log = NotFoundLog.Create(Tr, path, lastSeenAtUtc).Value;
        for (var i = 1; i < hitCount; i++)
        {
            log.RecordHit(lastSeenAtUtc);
        }

        _notFoundLogRepository.Seed(log);
        return log;
    }

    [Fact]
    public async Task ExecuteAsync_DeletesPathsOlderThan90DaysWithFewerThan5Hits()
    {
        var stale = Seed("nadir-404", DateTime.UtcNow.AddDays(-91), hitCount: 3);

        await CreateJob().ExecuteAsync(CancellationToken.None);

        Assert.DoesNotContain(stale, _notFoundLogRepository.NotFoundLogs);
    }

    [Fact]
    public async Task ExecuteAsync_LeavesPathsWithinThe90DayWindow_Untouched()
    {
        var recent = Seed("yeni-404", DateTime.UtcNow.AddDays(-89), hitCount: 1);

        await CreateJob().ExecuteAsync(CancellationToken.None);

        Assert.Contains(recent, _notFoundLogRepository.NotFoundLogs);
    }

    [Fact]
    public async Task ExecuteAsync_LeavesOldPathsWithAtLeast5Hits_Untouched()
    {
        var popular = Seed("populer-404", DateTime.UtcNow.AddDays(-120), hitCount: 5);

        await CreateJob().ExecuteAsync(CancellationToken.None);

        Assert.Contains(popular, _notFoundLogRepository.NotFoundLogs);
    }
}
