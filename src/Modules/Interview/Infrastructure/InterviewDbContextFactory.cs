using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GenclikMerkezi.Modules.Interview.Infrastructure;

public sealed class InterviewDbContextFactory : IDesignTimeDbContextFactory<InterviewDbContext>
{
    public InterviewDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__InterviewDatabase")
            ?? "Server=(localdb)\\mssqllocaldb;Database=GenclikMerkezi.Interview;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<InterviewDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new InterviewDbContext(optionsBuilder.Options, new DesignTimePublisher());
    }

    private sealed class DesignTimePublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;
    }
}
