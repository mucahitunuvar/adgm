using GenclikMerkezi.Modules.Website;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Infrastructure.Jobs;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.UnitTests.Website.Jobs;

// Hangfire runtime devreye girmez - job iki sahte repository (IEventScheduleRepository,
// IEventRegistrationRepository) ile bir ServiceCollection'dan çözülür ve ExecuteAsync doğrudan
// çağrılır, aynı desen diğer job testlerinde kullanılıyor. §1 "etkinlik bitiminden 365 gün sonra".
public class AnonymizeExpiredEventRegistrationsJobTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LegalDocumentKey PrivacyKey = LegalDocumentKey.Create("kvkk-event").Value;
    private static readonly DateTime Now = new(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeEventScheduleRepository _scheduleRepository = new();
    private readonly FakeEventRegistrationRepository _registrationRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private AnonymizeExpiredEventRegistrationsJob CreateJob(DateTime now)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IEventScheduleRepository>(_scheduleRepository);
        services.AddSingleton<IEventRegistrationRepository>(_registrationRepository);
        services.AddKeyedSingleton<IUnitOfWork>(WebsiteModuleMarker.UnitOfWorkKey, _unitOfWork);
        var serviceProvider = services.BuildServiceProvider();

        return new AnonymizeExpiredEventRegistrationsJob(serviceProvider.GetRequiredService<IServiceScopeFactory>(), new FixedTimeProvider(now));
    }

    private static EventSchedule CreateSchedule(DateTime endsAtUtc) =>
        EventSchedule.Create(
            Guid.NewGuid(), endsAtUtc.AddHours(-2), endsAtUtc, EventFormat.InPerson, null, null, false, null, null, null, null, false, false,
            Tr, null, null, null, null, null, null, Guid.NewGuid(), endsAtUtc.AddDays(-30)).Value;

    private static EventRegistration CreateConfirmedRegistration(Guid eventScheduleId, DateTime createdAtUtc)
    {
        var registration = EventRegistration.Create(
            eventScheduleId, Guid.NewGuid(), "Ahmet", "Yılmaz", $"katilimci-{Guid.NewGuid():N}@example.com", "5551234567", Guid.NewGuid(), Tr,
            PrivacyKey, 1, null, null, Guid.NewGuid().ToString("N"), createdAtUtc).Value;
        registration.ApplyCapacityDecision(EventCapacityDecision.Confirmed, "System", createdAtUtc);
        registration.ConfirmCapacityDecisionCommitted();
        return registration;
    }

    [Fact]
    public async Task ExecuteAsync_AnonymizesRegistration_WhenEventEndedExactly365DaysAgo()
    {
        var schedule = CreateSchedule(Now.AddDays(-365));
        _scheduleRepository.Seed(schedule);
        var registration = CreateConfirmedRegistration(schedule.Id, Now.AddDays(-400));
        _registrationRepository.Seed(registration);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.NotNull(registration.AnonymizedAtUtc);
    }

    // §1 "etkinlik bitiminden 364/366 gün" sınır testi.
    [Fact]
    public async Task ExecuteAsync_LeavesRegistrationUntouched_WhenEventEnded364DaysAgo()
    {
        var schedule = CreateSchedule(Now.AddDays(-364));
        _scheduleRepository.Seed(schedule);
        var registration = CreateConfirmedRegistration(schedule.Id, Now.AddDays(-400));
        _registrationRepository.Seed(registration);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.Null(registration.AnonymizedAtUtc);
    }

    [Fact]
    public async Task ExecuteAsync_AnonymizesRegistration_WhenEventEnded366DaysAgo()
    {
        var schedule = CreateSchedule(Now.AddDays(-366));
        _scheduleRepository.Seed(schedule);
        var registration = CreateConfirmedRegistration(schedule.Id, Now.AddDays(-400));
        _registrationRepository.Seed(registration);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.NotNull(registration.AnonymizedAtUtc);
    }

    // §1 "alanların temizlenmesi, kalan alanlar": FirstName/LastName/Email/Phone/UserId cleared;
    // Status, CreatedAtUtc and StatusHistory survive.
    [Fact]
    public async Task ExecuteAsync_ClearsPersonalFields_ButKeepsStatusDatesAndHistory()
    {
        var schedule = CreateSchedule(Now.AddDays(-400));
        _scheduleRepository.Seed(schedule);
        var registration = CreateConfirmedRegistration(schedule.Id, Now.AddDays(-430));
        var createdAtBefore = registration.CreatedAtUtc;
        var historyCountBefore = registration.StatusHistory.Count;
        _registrationRepository.Seed(registration);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.Equal(string.Empty, registration.FirstName);
        Assert.Equal(string.Empty, registration.LastName);
        Assert.Equal(string.Empty, registration.Email);
        Assert.Null(registration.Phone);
        Assert.Null(registration.UserId);
        Assert.Equal(EventRegistrationStatus.Confirmed, registration.Status);
        Assert.Equal(createdAtBefore, registration.CreatedAtUtc);
        Assert.Equal(historyCountBefore, registration.StatusHistory.Count);
    }

    // §1 "Anonimleştirilmiş kayıtta durum değiştirilemez" - once this job has run, the registration's
    // own domain methods must refuse further mutation.
    [Fact]
    public async Task ExecuteAsync_LeavesAnonymizedRegistrationUnmanageable()
    {
        var schedule = CreateSchedule(Now.AddDays(-400));
        _scheduleRepository.Seed(schedule);
        var registration = CreateConfirmedRegistration(schedule.Id, Now.AddDays(-430));
        _registrationRepository.Seed(registration);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        var result = registration.Cancel(EventRegistrationCancelledBy.Admin, Now);
        Assert.True(result.IsFailure);
        Assert.Equal("EventRegistration.Anonymized", result.Error.Code);
    }

    [Fact]
    public async Task ExecuteAsync_ProcessesAtMost500RegistrationsPerRun()
    {
        var schedule = CreateSchedule(Now.AddDays(-400));
        _scheduleRepository.Seed(schedule);
        for (var i = 0; i < 501; i++)
        {
            _registrationRepository.Seed(CreateConfirmedRegistration(schedule.Id, Now.AddDays(-430)));
        }

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        var stillDue = await _registrationRepository.GetDueForAnonymizationAsync([schedule.Id], int.MaxValue, CancellationToken.None);
        Assert.Single(stillDue);
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }
}
