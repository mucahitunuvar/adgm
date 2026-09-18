using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GenclikMerkezi.Modules.CareerAdvisor.Infrastructure;

public sealed class CareerAdvisorDbContextFactory : IDesignTimeDbContextFactory<CareerAdvisorDbContext>
{
    public CareerAdvisorDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__CareerAdvisorDatabase")
            ?? "Server=(localdb)\\mssqllocaldb;Database=GenclikMerkezi.CareerAdvisor;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<CareerAdvisorDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new CareerAdvisorDbContext(optionsBuilder.Options, new DesignTimePublisher());
    }

    private sealed class DesignTimePublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;
    }
}
