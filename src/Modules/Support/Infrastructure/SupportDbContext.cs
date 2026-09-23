using GenclikMerkezi.BuildingBlocks.Infrastructure.Events;
using GenclikMerkezi.Modules.Support.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Support.Infrastructure;

public sealed class SupportDbContext(DbContextOptions<SupportDbContext> options, IPublisher publisher)
    : DbContext(options), IUnitOfWork
{
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();

    public DbSet<SupportTicketMessage> SupportTicketMessages => Set<SupportTicketMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SupportDbContext).Assembly);
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
