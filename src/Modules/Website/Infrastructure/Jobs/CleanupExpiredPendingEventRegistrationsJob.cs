using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Jobs;

// ADR-024 §11.2 (Faz 4 Görev 5). Hangfire recurring job (Program.cs: RecurringJob.AddOrUpdate<
// CleanupExpiredPendingEventRegistrationsJob> ile saatlik kaydedilir), same singleton-safe-dependencies-
// only pattern the module's other jobs use. §1 "kayıt Rejected olmaz, silinme job'ına bırakılır": a
// verification link never clicked within EventRegistration.VerificationTokenLifetime (24h) never held
// capacity (PendingVerification is excluded from both ConfirmedCount/WaitlistedCount - see
// EventSchedule.ReserveCapacity), so deleting the row outright needs no counter adjustment.
public sealed class CleanupExpiredPendingEventRegistrationsJob(IServiceScopeFactory serviceScopeFactory, TimeProvider timeProvider)
{
    // §1 "Tek seferde en fazla 500."
    private const int MaxBatchSize = 500;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var eventRegistrationRepository = scope.ServiceProvider.GetRequiredService<IEventRegistrationRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(WebsiteModuleMarker.UnitOfWorkKey);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var createdBeforeUtc = now - EventRegistration.VerificationTokenLifetime;

        var expired = await eventRegistrationRepository.GetExpiredPendingVerificationAsync(createdBeforeUtc, MaxBatchSize, cancellationToken);
        foreach (var registration in expired)
        {
            eventRegistrationRepository.Remove(registration);
        }

        if (expired.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
