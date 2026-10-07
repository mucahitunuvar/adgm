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
// CleanupExpiredCookieConsentRecordsJobTests'te kullanılıyor. §1 "kayıt Rejected olmaz, silinme
// job'ına bırakılır" - PendingVerification hiçbir zaman kontenjan tutmadığından (EventSchedule
// ConfirmedCount/WaitlistedCount'u bu statüde hiç artmaz), bu job EventSchedule'a hiç dokunmaz;
// sayaçların değişmediği EventRegistrationManagementFlowTests tarafındaki eşdeğer entegrasyon
// senaryolarında zaten doğrulanıyor.
public class CleanupExpiredPendingEventRegistrationsJobTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LegalDocumentKey PrivacyKey = LegalDocumentKey.Create("kvkk-event").Value;
    private static readonly DateTime Now = new(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeEventRegistrationRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CleanupExpiredPendingEventRegistrationsJob CreateJob(DateTime now)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IEventRegistrationRepository>(_repository);
        services.AddKeyedSingleton<IUnitOfWork>(WebsiteModuleMarker.UnitOfWorkKey, _unitOfWork);
        var serviceProvider = services.BuildServiceProvider();

        return new CleanupExpiredPendingEventRegistrationsJob(
            serviceProvider.GetRequiredService<IServiceScopeFactory>(), new FixedTimeProvider(now));
    }

    private static EventRegistration CreatePending(DateTime createdAtUtc) =>
        EventRegistration.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Ahmet", "Yılmaz", $"katilimci-{Guid.NewGuid():N}@example.com", null, null, Tr, PrivacyKey, 1,
            "hash-value", createdAtUtc.AddHours(24), Guid.NewGuid().ToString("N"), createdAtUtc).Value;

    [Fact]
    public async Task ExecuteAsync_RemovesPendingVerificationCreatedExactly24HoursAgo()
    {
        var registration = CreatePending(Now.AddHours(-24));
        _repository.Seed(registration);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.Null(await _repository.GetByIdAsync(registration.Id));
    }

    // §1 "24 saat sınırı (23:59 ve 24:01)" - a registration created 23 hours and 59 minutes ago must
    // still be left untouched.
    [Fact]
    public async Task ExecuteAsync_LeavesPendingVerificationCreated23Hours59MinutesAgoUntouched()
    {
        var registration = CreatePending(Now.AddHours(-24).AddMinutes(1));
        _repository.Seed(registration);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.NotNull(await _repository.GetByIdAsync(registration.Id));
    }

    [Fact]
    public async Task ExecuteAsync_RemovesPendingVerificationCreated24Hours01MinutesAgo()
    {
        var registration = CreatePending(Now.AddHours(-24).AddMinutes(-1));
        _repository.Seed(registration);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.Null(await _repository.GetByIdAsync(registration.Id));
    }

    [Fact]
    public async Task ExecuteAsync_NeverRemovesAnActiveNonPendingRegistration()
    {
        var registration = CreatePending(Now.AddDays(-10));
        registration.ApplyCapacityDecision(EventCapacityDecision.Applied, "System", Now.AddDays(-10));
        _repository.Seed(registration);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.NotNull(await _repository.GetByIdAsync(registration.Id));
    }

    [Fact]
    public async Task ExecuteAsync_ProcessesAtMost500RegistrationsPerRun()
    {
        for (var i = 0; i < 501; i++)
        {
            _repository.Seed(CreatePending(Now.AddDays(-1)));
        }

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        var stillDue = await _repository.GetExpiredPendingVerificationAsync(Now, int.MaxValue, CancellationToken.None);
        Assert.Single(stillDue);
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }
}
