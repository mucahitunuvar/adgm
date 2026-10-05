using GenclikMerkezi.BuildingBlocks.Infrastructure.Events;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure;

public sealed class WebsiteDbContext(DbContextOptions<WebsiteDbContext> options, IPublisher publisher)
    : DbContext(options), IUnitOfWork
{
    public DbSet<SiteLanguage> SiteLanguages => Set<SiteLanguage>();

    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();

    public DbSet<SiteSettings> SiteSettingsEntries => Set<SiteSettings>();

    public DbSet<ContentType> ContentTypes => Set<ContentType>();

    public DbSet<ContentItem> ContentItems => Set<ContentItem>();

    public DbSet<Redirect> Redirects => Set<Redirect>();

    public DbSet<NotFoundLog> NotFoundLogs => Set<NotFoundLog>();

    public DbSet<Video> Videos => Set<Video>();

    public DbSet<ContentCategory> ContentCategories => Set<ContentCategory>();

    public DbSet<ContentTag> ContentTags => Set<ContentTag>();

    public DbSet<Menu> Menus => Set<Menu>();

    public DbSet<Slider> Sliders => Set<Slider>();

    public DbSet<Partner> Partners => Set<Partner>();

    public DbSet<ImpactMetric> ImpactMetrics => Set<ImpactMetric>();

    public DbSet<PageLayout> PageLayouts => Set<PageLayout>();

    public DbSet<Popup> Popups => Set<Popup>();

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
