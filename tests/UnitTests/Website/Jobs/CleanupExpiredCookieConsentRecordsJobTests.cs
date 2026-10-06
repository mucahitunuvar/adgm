using GenclikMerkezi.Modules.Website;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Infrastructure.Jobs;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.UnitTests.Website.Jobs;

// Hangfire runtime devreye girmez - job kendi IServiceScopeFactory'sini bir ServiceCollection'dan
// çözer, bu yüzden sahte repository buraya kaydedilir ve ExecuteAsync doğrudan çağrılır, aynı desen
// CleanupExpiredNewsletterSubscribersJobTests'te kullanılıyor.
public class CleanupExpiredCookieConsentRecordsJobTests
{
    private static readonly LegalDocumentKey PolicyKey = LegalDocumentKey.Create("cookie-policy").Value;
    private static readonly DateTime Now = new(2026, 10, 6, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeCookieConsentRecordRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CleanupExpiredCookieConsentRecordsJob CreateJob(DateTime now)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICookieConsentRecordRepository>(_repository);
        services.AddKeyedSingleton<IUnitOfWork>(WebsiteModuleMarker.UnitOfWorkKey, _unitOfWork);
        var serviceProvider = services.BuildServiceProvider();

        return new CleanupExpiredCookieConsentRecordsJob(serviceProvider.GetRequiredService<IServiceScopeFactory>(), new FixedTimeProvider(now));
    }

    private static CookieConsentRecord CreateRecord(DateTime recordedAtUtc) =>
        CookieConsentRecord.Create(Guid.NewGuid(), [], PolicyKey, 1, CookieConsentAction.AcceptAll, recordedAtUtc).Value;

    [Fact]
    public async Task ExecuteAsync_RemovesRecordsOlderThanRetentionWindow()
    {
        var record = CreateRecord(Now.AddDays(-CleanupExpiredCookieConsentRecordsJob.RetentionDays - 1));
        _repository.Seed(record);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.False(_repository.Contains(record.Id));
    }

    [Fact]
    public async Task ExecuteAsync_LeavesRecordsWithinRetentionWindowUntouched()
    {
        var record = CreateRecord(Now.AddDays(-CleanupExpiredCookieConsentRecordsJob.RetentionDays + 1));
        _repository.Seed(record);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.True(_repository.Contains(record.Id));
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }
}
