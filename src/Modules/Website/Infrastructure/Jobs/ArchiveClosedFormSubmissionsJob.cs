using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Jobs;

// ADR-024 §12.2 (Faz 3 Görev 5). Hangfire recurring job (Program.cs: RecurringJob.AddOrUpdate<
// ArchiveClosedFormSubmissionsJob> ile günlük kaydedilir), same singleton-safe-dependencies-only
// pattern CleanupStaleNotFoundLogsJob/PermanentlyDeleteExpiredTrashJob already use. "Açık başvurular
// arşivlenmez" is enforced by the repository query itself (ArchiveEligibleSinceUtc is only ever set
// while a submission is in a closing status), not by this job.
public sealed class ArchiveClosedFormSubmissionsJob(IServiceScopeFactory serviceScopeFactory, TimeProvider timeProvider)
{
    private const int ArchiveAfterDays = 30;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var formSubmissionRepository = scope.ServiceProvider.GetRequiredService<IFormSubmissionRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(WebsiteModuleMarker.UnitOfWorkKey);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var eligibleBefore = now.AddDays(-ArchiveAfterDays);

        var due = await formSubmissionRepository.GetDueForArchiveAsync(eligibleBefore, cancellationToken);
        if (due.Count == 0)
        {
            return;
        }

        foreach (var submission in due)
        {
            submission.Archive(now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
