using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Jobs;

// ADR-024 §12.2 (Faz 3 Görev 5). Hangfire recurring job (Program.cs: RecurringJob.AddOrUpdate<
// AnonymizeExpiredFormSubmissionsJob> ile günlük kaydedilir), same singleton-safe-dependencies-only
// pattern the module's other jobs use. Two phases, each its own SaveChangesAsync (ADR-024 §12.2
// "tek transaction'da" for the field-clearing, then "transaction commit'inden sonra" for the storage
// delete): first anonymize up to MaxBatchSize newly-due submissions, then attempt to delete every
// still-pending file (this run's new ones and any left over from a previous run's failed delete) - a
// failed delete simply stays pending for the next run, never retried in a loop within this same run.
public sealed class AnonymizeExpiredFormSubmissionsJob(
    IServiceScopeFactory serviceScopeFactory, TimeProvider timeProvider, ILogger<AnonymizeExpiredFormSubmissionsJob> logger)
{
    // §12.2 "Job, toplu işlemlerde tek seferde en fazla 500 başvuru işler."
    private const int MaxBatchSize = 500;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var formSubmissionRepository = scope.ServiceProvider.GetRequiredService<IFormSubmissionRepository>();
        var fileStorageService = scope.ServiceProvider.GetRequiredService<IFileStorageService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(WebsiteModuleMarker.UnitOfWorkKey);

        var now = timeProvider.GetUtcNow().UtcDateTime;

        var due = await formSubmissionRepository.GetDueForAnonymizationAsync(now, MaxBatchSize, cancellationToken);
        if (due.Count > 0)
        {
            foreach (var submission in due)
            {
                submission.Anonymize(now);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        await DeletePendingFilesAsync(formSubmissionRepository, fileStorageService, unitOfWork, cancellationToken);
    }

    private async Task DeletePendingFilesAsync(
        IFormSubmissionRepository formSubmissionRepository, IFileStorageService fileStorageService, IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var withPendingDeletions = await formSubmissionRepository.GetWithPendingFileDeletionsAsync(MaxBatchSize, cancellationToken);
        if (withPendingDeletions.Count == 0)
        {
            return;
        }

        var anyDeleted = false;

        foreach (var submission in withPendingDeletions)
        {
            foreach (var deletion in submission.PendingFileDeletions.ToList())
            {
                try
                {
                    await fileStorageService.DeleteAsync(deletion.FileKey, cancellationToken);
                    submission.RemovePendingFileDeletion(deletion);
                    anyDeleted = true;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning(
                        ex, "Failed to delete anonymized form submission file '{FileKey}'; it will be retried on the next run.",
                        deletion.FileKey);
                }
            }
        }

        if (anyDeleted)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
