using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Jobs;

// ADR-024 §14 (Faz 3 Görev 6). Hangfire recurring job (Program.cs: RecurringJob.AddOrUpdate<
// CleanupExpiredNewsletterSubscribersJob> ile günlük kaydedilir), same singleton-safe-dependencies-only
// pattern the module's other jobs use. Two unrelated retention windows swept in one daily job, exactly
// as the master prompt specifies ("Her ikisi tek bir günlük Hangfire job'ında"): a PendingConfirmation
// row that is never confirmed within 7 days, and an Unsubscribed row kept for 30 days after the
// unsubscribe (for any support/audit need) before being removed outright - unlike FormSubmission,
// there is no separate anonymization step here, since the whole row is deleted.
public sealed class CleanupExpiredNewsletterSubscribersJob(IServiceScopeFactory serviceScopeFactory, TimeProvider timeProvider)
{
    private const int PendingConfirmationExpiryDays = 7;
    private const int UnsubscribedRetentionDays = 30;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var newsletterSubscriberRepository = scope.ServiceProvider.GetRequiredService<INewsletterSubscriberRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(WebsiteModuleMarker.UnitOfWorkKey);

        var now = timeProvider.GetUtcNow().UtcDateTime;

        var expiredPending = await newsletterSubscriberRepository.GetPendingConfirmationOlderThanAsync(
            now.AddDays(-PendingConfirmationExpiryDays), cancellationToken);
        foreach (var subscriber in expiredPending)
        {
            newsletterSubscriberRepository.Remove(subscriber);
        }

        var expiredUnsubscribed = await newsletterSubscriberRepository.GetUnsubscribedOlderThanAsync(
            now.AddDays(-UnsubscribedRetentionDays), cancellationToken);
        foreach (var subscriber in expiredUnsubscribed)
        {
            newsletterSubscriberRepository.Remove(subscriber);
        }

        if (expiredPending.Count > 0 || expiredUnsubscribed.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
