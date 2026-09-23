using GenclikMerkezi.Modules.CareerDevelopment.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.CareerDevelopment.Infrastructure.Configurations;

public sealed class CareerGoalConfiguration : IEntityTypeConfiguration<CareerGoal>
{
    public void Configure(EntityTypeBuilder<CareerGoal> builder)
    {
        builder.ToTable("CareerGoals");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).ValueGeneratedNever();

        builder.Property(g => g.CandidateCvId).IsRequired();
        builder.HasIndex(g => g.CandidateCvId);

        builder.Property(g => g.Description).HasMaxLength(2000).IsRequired();
        builder.Property(g => g.TargetPositionId);
        builder.Property(g => g.SetByAdvisorId).IsRequired();
        builder.Property(g => g.CreatedAtUtc).IsRequired();
    }
}
