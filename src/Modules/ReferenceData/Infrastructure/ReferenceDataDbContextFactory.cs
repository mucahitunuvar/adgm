using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure;

public sealed class ReferenceDataDbContextFactory : IDesignTimeDbContextFactory<ReferenceDataDbContext>
{
    public ReferenceDataDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__ReferenceDataDatabase")
            ?? "Server=(localdb)\\mssqllocaldb;Database=GenclikMerkezi.ReferenceData;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<ReferenceDataDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ReferenceDataDbContext(optionsBuilder.Options);
    }
}
