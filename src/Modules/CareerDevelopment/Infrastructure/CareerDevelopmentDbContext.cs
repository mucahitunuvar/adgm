using GenclikMerkezi.BuildingBlocks.Infrastructure.Events;
using GenclikMerkezi.Modules.CareerDevelopment.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.CareerDevelopment.Infrastructure;

public sealed class CareerDevelopmentDbContext(DbContextOptions<CareerDevelopmentDbContext> options, IPublisher publisher)
    : DbContext(options), IUnitOfWork
{
    public DbSet<SkillGap> SkillGaps => Set<SkillGap>();

    public DbSet<CareerGoal> CareerGoals => Set<CareerGoal>();

    public DbSet<TrainingRecommendation> TrainingRecommendations => Set<TrainingRecommendation>();

    public DbSet<AdvisorRecommendation> AdvisorRecommendations => Set<AdvisorRecommendation>();

    public DbSet<DevelopmentPlan> DevelopmentPlans => Set<DevelopmentPlan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CareerDevelopmentDbContext).Assembly);
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
