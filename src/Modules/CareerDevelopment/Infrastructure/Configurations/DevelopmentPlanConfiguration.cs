using GenclikMerkezi.Modules.CareerDevelopment.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.CareerDevelopment.Infrastructure.Configurations;

public sealed class DevelopmentPlanConfiguration : IEntityTypeConfiguration<DevelopmentPlan>
{
    public void Configure(EntityTypeBuilder<DevelopmentPlan> builder)
    {
        builder.ToTable("DevelopmentPlans");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.CandidateCvId).IsRequired();
        builder.HasIndex(p => p.CandidateCvId);

        builder.Property(p => p.SkillGapId);
        builder.Property(p => p.CareerGoalId);
        builder.Property(p => p.Description).HasMaxLength(2000).IsRequired();

        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(p => p.CreatedByAdvisorId).IsRequired();
        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.CompletedAtUtc);
    }
}
