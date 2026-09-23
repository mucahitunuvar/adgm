using GenclikMerkezi.Modules.CareerDevelopment.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.CareerDevelopment.Infrastructure.Configurations;

public sealed class TrainingRecommendationConfiguration : IEntityTypeConfiguration<TrainingRecommendation>
{
    public void Configure(EntityTypeBuilder<TrainingRecommendation> builder)
    {
        builder.ToTable("TrainingRecommendations");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.CandidateCvId).IsRequired();
        builder.HasIndex(t => t.CandidateCvId);

        builder.Property(t => t.DevelopmentPlanId);
        builder.Property(t => t.TrainingId).IsRequired();
        builder.Property(t => t.RecommendedByAdvisorId).IsRequired();
        builder.Property(t => t.Notes).HasMaxLength(1000);
        builder.Property(t => t.RecommendedAtUtc).IsRequired();
    }
}
