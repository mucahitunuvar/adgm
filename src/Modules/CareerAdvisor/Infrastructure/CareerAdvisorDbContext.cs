using GenclikMerkezi.BuildingBlocks.Infrastructure.Events;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.CareerAdvisor.Infrastructure;

public sealed class CareerAdvisorDbContext(DbContextOptions<CareerAdvisorDbContext> options, IPublisher publisher)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Domain.CareerAdvisor> CareerAdvisors => Set<Domain.CareerAdvisor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CareerAdvisorDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregatesWithEvents = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        await DomainEventDispatcher.DispatchAndClearEventsAsync(aggregatesWithEvents, publisher, cancellationToken);

        return result;
    }
}
