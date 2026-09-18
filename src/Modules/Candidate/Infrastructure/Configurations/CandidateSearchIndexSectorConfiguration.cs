using GenclikMerkezi.Modules.Candidate.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure.Configurations;

public sealed class CandidateSearchIndexSectorConfiguration : IEntityTypeConfiguration<CandidateSearchIndexSector>
{
    public void Configure(EntityTypeBuilder<CandidateSearchIndexSector> builder)
    {
        builder.ToTable("CandidateSearchIndexSectors");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.CandidateSearchIndexId).IsRequired();
        builder.Property(s => s.SectorId).IsRequired();

        builder.HasIndex(s => s.SectorId);
        builder.HasIndex(s => new { s.CandidateSearchIndexId, s.SectorId }).IsUnique();
    }
}
