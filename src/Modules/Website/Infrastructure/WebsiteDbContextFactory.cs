using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GenclikMerkezi.Modules.Website.Infrastructure;

public sealed class WebsiteDbContextFactory : IDesignTimeDbContextFactory<WebsiteDbContext>
{
    public WebsiteDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__WebsiteDatabase")
            ?? "Server=(localdb)\\mssqllocaldb;Database=GenclikMerkezi.Website;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<WebsiteDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new WebsiteDbContext(optionsBuilder.Options, new DesignTimePublisher());
    }

    private sealed class DesignTimePublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;
    }
}
