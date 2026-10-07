using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Jobs;

// ADR-024 §11.2 (Faz 4 Görev 5). Hangfire recurring job (Program.cs: RecurringJob.AddOrUpdate<
// AnonymizeExpiredEventRegistrationsJob> ile günlük kaydedilir). §1 "etkinlik bitiminden 365 gün sonra":
// unlike AnonymizeExpiredFormSubmissionsJob's own per-form RetentionDays (read from FormDefinition),
// every EventSchedule shares the same fixed window (RetentionDays below - SECURITY.md'ye Görev 6'da
// yazılır, hukuk danışmanınca teyit edilmesi gerekir), so this job computes the single cutoff itself
// (mirrors CleanupExpiredCookieConsentRecordsJob/CleanupExpiredNewsletterSubscribersJob) rather than
// asking a repository to compute a per-row one. Two repositories, same split every other
// EventRegistration use case already keeps: IEventScheduleRepository only ever answers "which
// schedules ended before X", IEventRegistrationRepository stays the only one that ever
// mutates/removes a registration row.
public sealed class AnonymizeExpiredEventRegistrationsJob(IServiceScopeFactory serviceScopeFactory, TimeProvider timeProvider)
{
    // §1 "Süre kodda sabittir, SECURITY.md'ye yazılır ve hukuk danışmanınca teyit edilmelidir."
    public const int RetentionDays = 365;

    // §1 "Tek seferde en fazla 500."
    private const int MaxBatchSize = 500;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var eventScheduleRepository = scope.ServiceProvider.GetRequiredService<IEventScheduleRepository>();
        var eventRegistrationRepository = scope.ServiceProvider.GetRequiredService<IEventRegistrationRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(WebsiteModuleMarker.UnitOfWorkKey);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var endedBeforeUtc = now.AddDays(-RetentionDays);

        var eventScheduleIds = await eventScheduleRepository.GetIdsEndedBeforeAsync(endedBeforeUtc, cancellationToken);
        if (eventScheduleIds.Count == 0)
        {
            return;
        }

        var due = await eventRegistrationRepository.GetDueForAnonymizationAsync(eventScheduleIds, MaxBatchSize, cancellationToken);
        if (due.Count == 0)
        {
            return;
        }

        foreach (var registration in due)
        {
            registration.Anonymize(now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
