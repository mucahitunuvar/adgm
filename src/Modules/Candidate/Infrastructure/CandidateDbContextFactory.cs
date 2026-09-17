using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure;

public sealed class CandidateDbContextFactory : IDesignTimeDbContextFactory<CandidateDbContext>
{
    public CandidateDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__CandidateDatabase")
            ?? "Server=(localdb)\\mssqllocaldb;Database=GenclikMerkezi.Candidate;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<CandidateDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new CandidateDbContext(optionsBuilder.Options);
    }
}
