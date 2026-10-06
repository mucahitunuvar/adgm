using GenclikMerkezi.Modules.Website.Application.Events;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace GenclikMerkezi.UnitTests.Website.Application.Events;

// ADR-024 §11.2 (Faz 4 Görev 4) - §1 "en fazla 200 alıcıyı tek seferde işler, fazlası için Hangfire
// job'ı ile parçalanır". Driving 200+ real registrations through an HTTP integration test is
// impractical, so the chunking boundary is verified here against a fake repository instead; the
// recipient-set correctness (which statuses get notified) is covered both here and by the integration
// suite's smaller-scale end-to-end fan-out test.
public class EventCancellationNotifierTests
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

    private EventCancellationNotifier CreateNotifier() =>
        new(
            _eventScheduleRepository, _contentItemRepository, _eventRegistrationRepository,
            new EventRegistrationNotifier(_emailSender, EmptyConfiguration()), _batchScheduler,
            NullLogger<EventCancellationNotifier>.Instance);

    [Fact]
    public async Task NotifyCancellationAsync_NotifiesOnlyApplicableStatuses_AndDoesNotScheduleContinuation()
    {
        var eventSchedule = SeedCancelledEventSchedule();
        SeedContentItem();
        Seed(EventRegistrationStatus.Applied, "applied@example.com");
        Seed(EventRegistrationStatus.Confirmed, "confirmed@example.com");
        Seed(EventRegistrationStatus.Waitlisted, "waitlisted@example.com");
        Seed(EventRegistrationStatus.Rejected, "rejected@example.com");
        Seed(EventRegistrationStatus.PendingVerification, "pending@example.com");

        await CreateNotifier().NotifyCancellationAsync(eventSchedule, CancellationToken.None);

        var notifiedEmails = _emailSender.SentEmails.Select(e => e.RecipientEmail).ToList();
        Assert.Equal(3, notifiedEmails.Count);
        Assert.Contains("applied@example.com", notifiedEmails);
        Assert.Contains("confirmed@example.com", notifiedEmails);
        Assert.Contains("waitlisted@example.com", notifiedEmails);
        Assert.DoesNotContain("rejected@example.com", notifiedEmails);
        Assert.DoesNotContain("pending@example.com", notifiedEmails);
        Assert.Empty(_batchScheduler.ScheduledBatches);
    }

    [Fact]
    public async Task NotifyCancellationAsync_WithExactlyBatchSizeRecipients_SchedulesNextBatch()
    {
        var eventSchedule = SeedCancelledEventSchedule();
        SeedContentItem();
        for (var i = 0; i < EventCancellationNotifier.BatchSize; i++)
        {
            Seed(EventRegistrationStatus.Applied, $"recipient{i}@example.com");
        }

        await CreateNotifier().NotifyCancellationAsync(eventSchedule, CancellationToken.None);

        Assert.Equal(EventCancellationNotifier.BatchSize, _emailSender.SentEmails.Count);
        var scheduled = Assert.Single(_batchScheduler.ScheduledBatches);
        Assert.Equal(ContentItemId, scheduled.ContentItemId);
        Assert.Equal(EventCancellationNotifier.BatchSize, scheduled.Skip);
    }

    [Fact]
    public async Task NotifyCancellationAsync_WithFewerThanBatchSizeRecipients_DoesNotScheduleContinuation()
    {
        var eventSchedule = SeedCancelledEventSchedule();
        SeedContentItem();
        Seed(EventRegistrationStatus.Applied, "only-one@example.com");

        await CreateNotifier().NotifyCancellationAsync(eventSchedule, CancellationToken.None);

        Assert.Single(_emailSender.SentEmails);
        Assert.Empty(_batchScheduler.ScheduledBatches);
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenEventIsNotCancelled_SendsNothing()
    {
        var eventSchedule = EventSchedule.Create(
            ContentItemId, Now.AddDays(10), Now.AddDays(10).AddHours(2), EventFormat.InPerson, null, null, registrationEnabled: true,
            registrationOpensAtUtc: null, registrationClosesAtUtc: null, minAge: null, maxAge: null, autoConfirm: true,
            waitlistEnabled: true, Tr, "Salon", "Adres", "Ücretsiz", "Eğitmen", "<p/>", "Not", UserId, Now).Value;
        _eventScheduleRepository.Add(eventSchedule);
        SeedContentItem();
        Seed(EventRegistrationStatus.Applied, "applied@example.com");

        await CreateNotifier().ProcessBatchAsync(ContentItemId, skip: 0, CancellationToken.None);

        Assert.Empty(_emailSender.SentEmails);
    }

    private EventSchedule SeedCancelledEventSchedule()
    {
        var eventSchedule = EventSchedule.Create(
            ContentItemId, Now.AddDays(10), Now.AddDays(10).AddHours(2), EventFormat.InPerson, null, null, registrationEnabled: true,
            registrationOpensAtUtc: null, registrationClosesAtUtc: null, minAge: null, maxAge: null, autoConfirm: true,
            waitlistEnabled: true, Tr, "Salon", "Adres", "Ücretsiz", "Eğitmen", "<p/>", "Not", UserId, Now).Value;
        eventSchedule.Cancel("Hava koşulları", UserId, Now);
        _eventScheduleRepository.Add(eventSchedule);
        return eventSchedule;
    }

    private void SeedContentItem()
    {
        var contentItem = ContentItem.Create(
            Guid.NewGuid(), null, false, 1, false, null, null, Tr, "Yaz Kampı", "yaz-kampi", "etkinlikler", [], null, "<p/>",
            SeoMetadata.CreateEmpty(), UserId, Now).Value;
        _contentItemRepository.Seed(contentItem);
    }

    private void Seed(EventRegistrationStatus status, string email)
    {
        var registration = EventRegistration.Create(
            Guid.NewGuid(), ContentItemId, "Ahmet", "Yılmaz", email, null, null, Tr, PrivacyKey, 1, null, null,
            Guid.NewGuid().ToString(), Now).Value;

        if (status != EventRegistrationStatus.PendingVerification)
        {
            var decision = status switch
            {
                EventRegistrationStatus.Confirmed => EventCapacityDecision.Confirmed,
                EventRegistrationStatus.Waitlisted => EventCapacityDecision.Waitlisted,
                _ => EventCapacityDecision.Applied,
            };
            registration.ApplyCapacityDecision(decision, "System", Now);

            if (status == EventRegistrationStatus.Rejected)
            {
                registration.Reject("admin-1", Now);
            }
        }

        _eventRegistrationRepository.Add(registration);
    }

    private static IConfiguration EmptyConfiguration() =>
        new ConfigurationBuilder().Build();
}
