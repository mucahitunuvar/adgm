using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GenclikMerkezi.Modules.Matching.Infrastructure;

public sealed class MatchingDbContextFactory : IDesignTimeDbContextFactory<MatchingDbContext>
{
    public MatchingDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__MatchingDatabase")
            ?? "Server=(localdb)\\mssqllocaldb;Database=GenclikMerkezi.Matching;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<MatchingDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new MatchingDbContext(optionsBuilder.Options, new DesignTimePublisher());
    }

    private sealed class DesignTimePublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;
    }
}
