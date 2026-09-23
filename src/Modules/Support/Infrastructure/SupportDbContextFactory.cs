using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GenclikMerkezi.Modules.Support.Infrastructure;

public sealed class SupportDbContextFactory : IDesignTimeDbContextFactory<SupportDbContext>
{
    public SupportDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SupportDatabase")
            ?? "Server=(localdb)\\mssqllocaldb;Database=GenclikMerkezi.Support;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<SupportDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new SupportDbContext(optionsBuilder.Options, new DesignTimePublisher());
    }

    private sealed class DesignTimePublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;
    }
}
