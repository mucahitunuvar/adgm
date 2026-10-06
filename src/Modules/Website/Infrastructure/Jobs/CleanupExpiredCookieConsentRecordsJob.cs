using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Jobs;

// ADR-024 §13/SECURITY.md (Faz 3 Görev 7). Hangfire recurring job (Program.cs: RecurringJob.AddOrUpdate<
// CleanupExpiredCookieConsentRecordsJob> ile günlük kaydedilir), same singleton-safe-dependencies-only
// pattern the module's other jobs use. "Kayıtlar 3 yıl saklanır" - a fixed retention window, since
// CookieConsentRecord carries no personal data to anonymize (SECURITY.md), only the whole row to delete
// once it ages out.
public sealed class CleanupExpiredCookieConsentRecordsJob(IServiceScopeFactory serviceScopeFactory, TimeProvider timeProvider)
{
    public const int RetentionDays = 1095;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var cookieConsentRecordRepository = scope.ServiceProvider.GetRequiredService<ICookieConsentRecordRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(WebsiteModuleMarker.UnitOfWorkKey);

        var now = timeProvider.GetUtcNow().UtcDateTime;

        var expired = await cookieConsentRecordRepository.GetRecordedBeforeAsync(now.AddDays(-RetentionDays), cancellationToken);
        foreach (var record in expired)
        {
            cookieConsentRecordRepository.Remove(record);
        }

        if (expired.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
