using GenclikMerkezi.Modules.Website;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.Events;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Infrastructure.Jobs;
using GenclikMerkezi.Contracts.Website;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GenclikMerkezi.UnitTests.Website.Jobs;

// Hangfire runtime devreye girmez - job kendi IServiceScopeFactory'sini bir ServiceCollection'dan
// çözer ve ExecuteAsync doğrudan çağrılır, diğer Website job testleriyle aynı desen.
public class ProcessEventCancellationNotificationsJobTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LegalDocumentKey PrivacyKey = LegalDocumentKey.Create("kvkk-event").Value;
    private static readonly Guid ContentItemId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeEventScheduleRepository _eventScheduleRepository = new();
    private readonly FakeContentItemRepository _contentItemRepository = new();
    private readonly FakeEventRegistrationRepository _eventRegistrationRepository = new();
    private readonly FakeWebsiteEmailSender _emailSender = new();
    private readonly FakeEventCancellationBatchScheduler _batchScheduler = new();

    private ProcessEventCancellationNotificationsJob CreateJob()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IEventScheduleRepository>(_eventScheduleRepository);
        services.AddSingleton<IContentItemRepository>(_contentItemRepository);
        services.AddSingleton<IEventRegistrationRepository>(_eventRegistrationRepository);
        services.AddSingleton<IWebsiteEmailSender>(_emailSender);
        services.AddSingleton<IEventCancellationBatchScheduler>(_batchScheduler);
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddSingleton<ILogger<EventCancellationNotifier>>(NullLogger<EventCancellationNotifier>.Instance);
        services.AddSingleton<EventRegistrationNotifier>();
        services.AddSingleton<EventCancellationNotifier>();
        var serviceProvider = services.BuildServiceProvider();

        return new ProcessEventCancellationNotificationsJob(serviceProvider.GetRequiredService<IServiceScopeFactory>());
    }

    [Fact]
    public async Task ExecuteAsync_WithOverflowBatch_SendsNoticesAndSchedulesNextContinuation()
    {
        var eventSchedule = EventSchedule.Create(
            ContentItemId, Now.AddDays(10), Now.AddDays(10).AddHours(2), EventFormat.InPerson, null, null, registrationEnabled: true,
            registrationOpensAtUtc: null, registrationClosesAtUtc: null, minAge: null, maxAge: null, autoConfirm: true,
            waitlistEnabled: true, Tr, "Salon", "Adres", "Ücretsiz", "Eğitmen", "<p/>", "Not", UserId, Now).Value;
        eventSchedule.Cancel("Hava koşulları", UserId, Now);
        _eventScheduleRepository.Add(eventSchedule);

        var contentItem = ContentItem.Create(
            Guid.NewGuid(), null, false, 1, false, null, null, Tr, "Yaz Kampı", "yaz-kampi", "etkinlikler", [], null, "<p/>",
            SeoMetadata.CreateEmpty(), UserId, Now).Value;
        _contentItemRepository.Seed(contentItem);

        for (var i = 0; i < EventCancellationNotifier.BatchSize; i++)
        {
            var registration = EventRegistration.Create(
                Guid.NewGuid(), ContentItemId, "Ahmet", "Yılmaz", $"recipient{i}@example.com", null, null, Tr, PrivacyKey, 1, null, null,
                Guid.NewGuid().ToString(), Now).Value;
            registration.ApplyCapacityDecision(EventCapacityDecision.Applied, "System", Now);
            _eventRegistrationRepository.Add(registration);
        }

        await CreateJob().ExecuteAsync(ContentItemId, skip: 0, CancellationToken.None);

        Assert.Equal(EventCancellationNotifier.BatchSize, _emailSender.SentEmails.Count);
        var scheduled = Assert.Single(_batchScheduler.ScheduledBatches);
        Assert.Equal(EventCancellationNotifier.BatchSize, scheduled.Skip);
    }

    [Fact]
    public async Task ExecuteAsync_WithPartialBatch_SendsNoticesWithoutSchedulingFurtherContinuation()
    {
        var eventSchedule = EventSchedule.Create(
            ContentItemId, Now.AddDays(10), Now.AddDays(10).AddHours(2), EventFormat.InPerson, null, null, registrationEnabled: true,
            registrationOpensAtUtc: null, registrationClosesAtUtc: null, minAge: null, maxAge: null, autoConfirm: true,
            waitlistEnabled: true, Tr, "Salon", "Adres", "Ücretsiz", "Eğitmen", "<p/>", "Not", UserId, Now).Value;
        eventSchedule.Cancel("Hava koşulları", UserId, Now);
        _eventScheduleRepository.Add(eventSchedule);

        var contentItem = ContentItem.Create(
            Guid.NewGuid(), null, false, 1, false, null, null, Tr, "Yaz Kampı", "yaz-kampi", "etkinlikler", [], null, "<p/>",
            SeoMetadata.CreateEmpty(), UserId, Now).Value;
        _contentItemRepository.Seed(contentItem);

        var registration = EventRegistration.Create(
            Guid.NewGuid(), ContentItemId, "Ahmet", "Yılmaz", "last@example.com", null, null, Tr, PrivacyKey, 1, null, null,
            Guid.NewGuid().ToString(), Now).Value;
        registration.ApplyCapacityDecision(EventCapacityDecision.Applied, "System", Now);
        _eventRegistrationRepository.Add(registration);

        // A continuation call receiving fewer than BatchSize rows for the given skip (here, simply
        // skip: 0 against a single seeded row) must not enqueue a further continuation.
        await CreateJob().ExecuteAsync(ContentItemId, skip: 0, CancellationToken.None);

        Assert.Single(_emailSender.SentEmails);
        Assert.Empty(_batchScheduler.ScheduledBatches);
    }
}
