using GenclikMerkezi.BuildingBlocks.Infrastructure.Events;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure;

public sealed class WebsiteDbContext(DbContextOptions<WebsiteDbContext> options, IPublisher publisher)
    : DbContext(options), IUnitOfWork
{
    // No aggregates yet - Faz 0 Görev 1 only scaffolds the module. DbSets are added as each
    // aggregate (SiteLanguage, MediaAsset, SiteSettings, ...) lands in later Görevs.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WebsiteDbContext).Assembly);
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
