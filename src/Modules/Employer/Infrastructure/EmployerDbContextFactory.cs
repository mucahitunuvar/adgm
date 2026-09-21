using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GenclikMerkezi.Modules.Employer.Infrastructure;

public sealed class EmployerDbContextFactory : IDesignTimeDbContextFactory<EmployerDbContext>
{
    public EmployerDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__EmployerDatabase")
            ?? "Server=(localdb)\\mssqllocaldb;Database=GenclikMerkezi.Employer;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<EmployerDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new EmployerDbContext(optionsBuilder.Options, new DesignTimePublisher());
    }

    private sealed class DesignTimePublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;
    }
}
