using GenclikMerkezi.BuildingBlocks.Infrastructure.Events;
using GenclikMerkezi.Modules.Matching.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Matching.Infrastructure;

public sealed class MatchingDbContext(DbContextOptions<MatchingDbContext> options, IPublisher publisher)
    : DbContext(options), IUnitOfWork
{
    public DbSet<CandidateSuggestion> CandidateSuggestions => Set<CandidateSuggestion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MatchingDbContext).Assembly);
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
