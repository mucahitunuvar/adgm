using GenclikMerkezi.Modules.Candidate.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure.Configurations;

public sealed class CandidateSearchIndexConfiguration : IEntityTypeConfiguration<CandidateSearchIndex>
{
    public void Configure(EntityTypeBuilder<CandidateSearchIndex> builder)
    {
        builder.ToTable("CandidateSearchIndex");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.FullNameNormalized).IsRequired().HasMaxLength(400);
        builder.Property(i => i.Email).IsRequired().HasMaxLength(320);
        builder.Property(i => i.CompletionPercentage).IsRequired();
        builder.Property(i => i.UpdatedAtUtc).IsRequired();

        // Görev 2's listing endpoint filters on all of these.
        builder.HasIndex(i => i.FullNameNormalized);
        builder.HasIndex(i => i.ProvinceId);
        builder.HasIndex(i => i.DistrictId);
        builder.HasIndex(i => i.CompletionPercentage);

        builder.HasMany(i => i.EducationLevels)
            .WithOne()
            .HasForeignKey(e => e.CandidateSearchIndexId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(i => i.EducationLevels).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(i => i.Sectors)
            .WithOne()
            .HasForeignKey(s => s.CandidateSearchIndexId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(i => i.Sectors).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
