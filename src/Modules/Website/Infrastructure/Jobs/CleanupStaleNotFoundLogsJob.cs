using GenclikMerkezi.Modules.Website.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Jobs;

// ADR-024 §15 (Faz 1a Görev 5). Hangfire recurring job (Program.cs: RecurringJob.AddOrUpdate<
// CleanupStaleNotFoundLogsJob> ile günlük kaydedilir - Support modülünün CloseOverdueSupportTicketsJob
// deseniyle aynı: Hangfire job'ları bir HTTP request scope'unda çalışmaz, bu yüzden yalnızca
// singleton-güvenli IServiceScopeFactory constructor'da alınır ve scoped repository/UnitOfWork her
// çalıştırmada kendi scope'undan çözülür).
public sealed class CleanupStaleNotFoundLogsJob(IServiceScopeFactory serviceScopeFactory, TimeProvider timeProvider)
{
    private const int StaleAfterDays = 90;
    private const int MaxHitCountToDelete = 5;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var notFoundLogRepository = scope.ServiceProvider.GetRequiredService<INotFoundLogRepository>();

        var threshold = timeProvider.GetUtcNow().UtcDateTime.AddDays(-StaleAfterDays);
        await notFoundLogRepository.DeleteStaleAsync(threshold, MaxHitCountToDelete, cancellationToken);
    }
}
