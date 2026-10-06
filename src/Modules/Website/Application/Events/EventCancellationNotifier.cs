using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.Extensions.Logging;

namespace GenclikMerkezi.Modules.Website.Application.Events;

// ADR-024 §11.2 (Faz 4 Görev 4): IEventCancellationNotifier's real implementation, replacing Görev 1's
// NoOpEventCancellationNotifier. §1 "Gönderim commit sonrası, best-effort, en fazla 200 alıcıyı tek
// seferde işler, fazlası için Hangfire job'ı ile parçalanır" - NotifyCancellationAsync (called from
// CancelEventScheduleCommandHandler, already after its own SaveChangesAsync/cache invalidation) always
// processes the first batch inline; ProcessBatchAsync is also the entry point
// ProcessEventCancellationNotificationsJob calls for every later, Hangfire-driven chunk, so the
// "load batch, send, maybe enqueue the rest" logic exists exactly once.
public sealed class EventCancellationNotifier(
    IEventScheduleRepository eventScheduleRepository,
    IContentItemRepository contentItemRepository,
    IEventRegistrationRepository eventRegistrationRepository,
    EventRegistrationNotifier notifier,
    IEventCancellationBatchScheduler batchScheduler,
    ILogger<EventCancellationNotifier> logger)
    : IEventCancellationNotifier
{
    public const int BatchSize = 200;

    public Task NotifyCancellationAsync(EventSchedule eventSchedule, CancellationToken cancellationToken = default) =>
        ProcessBatchAsync(eventSchedule.ContentItemId, skip: 0, cancellationToken);

    public async Task ProcessBatchAsync(Guid contentItemId, int skip, CancellationToken cancellationToken)
    {
        var eventSchedule = await eventScheduleRepository.GetByContentItemIdAsync(contentItemId, cancellationToken);
        if (eventSchedule is null || !eventSchedule.IsCancelled)
        {
            // Defensive only - the event may have been reactivated between the triggering commit and a
            // later Hangfire-driven chunk actually running.
            return;
        }

        var contentItem = await contentItemRepository.GetByIdAsync(contentItemId, cancellationToken);
        var eventTitle = contentItem?.Translations.FirstOrDefault()?.Title ?? string.Empty;

        var recipients = await eventRegistrationRepository.GetCancellationRecipientsAsync(
            contentItemId, skip, BatchSize, cancellationToken);

        foreach (var recipient in recipients)
        {
            try
            {
                await notifier.SendEventCancellationNoticeAsync(
                    recipient.Email, recipient.LanguageCode, eventTitle, eventSchedule.CancellationReason, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(
                    ex, "Failed to send event cancellation notice for content item '{ContentItemId}'.", contentItemId);
            }
        }

        if (recipients.Count == BatchSize)
        {
            batchScheduler.ScheduleNextBatch(contentItemId, skip + BatchSize);
        }
    }
}
