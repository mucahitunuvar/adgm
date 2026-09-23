using GenclikMerkezi.Modules.CareerDevelopment.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.CareerDevelopment.Infrastructure.Configurations;

public sealed class SkillGapConfiguration : IEntityTypeConfiguration<SkillGap>
{
    public void Configure(EntityTypeBuilder<SkillGap> builder)
    {
        builder.ToTable("SkillGaps");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.CandidateCvId).IsRequired();
        builder.HasIndex(s => s.CandidateCvId);

        builder.Property(s => s.SkillId).IsRequired();
        builder.Property(s => s.IdentifiedByAdvisorId).IsRequired();
        builder.Property(s => s.Notes).HasMaxLength(1000);
        builder.Property(s => s.IdentifiedAtUtc).IsRequired();
    }
}
