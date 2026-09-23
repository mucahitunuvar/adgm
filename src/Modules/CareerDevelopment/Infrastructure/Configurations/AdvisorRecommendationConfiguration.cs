using GenclikMerkezi.Modules.CareerDevelopment.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.CareerDevelopment.Infrastructure.Configurations;

public sealed class AdvisorRecommendationConfiguration : IEntityTypeConfiguration<AdvisorRecommendation>
{
    public void Configure(EntityTypeBuilder<AdvisorRecommendation> builder)
    {
        builder.ToTable("AdvisorRecommendations");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.CandidateCvId).IsRequired();
        builder.HasIndex(a => a.CandidateCvId);

        builder.Property(a => a.AdvisorId).IsRequired();
        builder.Property(a => a.Content).HasMaxLength(4000).IsRequired();
        builder.Property(a => a.CreatedAtUtc).IsRequired();
    }
}
