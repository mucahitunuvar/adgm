using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GenclikMerkezi.Modules.CareerDevelopment.Infrastructure;

public sealed class CareerDevelopmentDbContextFactory : IDesignTimeDbContextFactory<CareerDevelopmentDbContext>
{
    public CareerDevelopmentDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__CareerDevelopmentDatabase")
            ?? "Server=(localdb)\\mssqllocaldb;Database=GenclikMerkezi.CareerDevelopment;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<CareerDevelopmentDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new CareerDevelopmentDbContext(optionsBuilder.Options, new DesignTimePublisher());
    }

    private sealed class DesignTimePublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;
    }
}
