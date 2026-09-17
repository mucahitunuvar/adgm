using GenclikMerkezi.Modules.Candidate.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure;

public sealed class CandidateDbContext(DbContextOptions<CandidateDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<CandidateCv> CandidateCvs => Set<CandidateCv>();

    public DbSet<CandidateCvContent> CandidateCvContents => Set<CandidateCvContent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CandidateDbContext).Assembly);
    }
}
