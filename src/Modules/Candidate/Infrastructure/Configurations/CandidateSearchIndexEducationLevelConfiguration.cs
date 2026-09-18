using GenclikMerkezi.Modules.Candidate.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure.Configurations;

public sealed class CandidateSearchIndexEducationLevelConfiguration : IEntityTypeConfiguration<CandidateSearchIndexEducationLevel>
{
    public void Configure(EntityTypeBuilder<CandidateSearchIndexEducationLevel> builder)
    {
        builder.ToTable("CandidateSearchIndexEducationLevels");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.CandidateSearchIndexId).IsRequired();
        builder.Property(e => e.EducationLevelId).IsRequired();

        builder.HasIndex(e => e.EducationLevelId);
        builder.HasIndex(e => new { e.CandidateSearchIndexId, e.EducationLevelId }).IsUnique();
    }
}
