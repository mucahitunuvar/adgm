using GenclikMerkezi.Modules.Candidate.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure.Configurations;

public sealed class EducationConfiguration : IEntityTypeConfiguration<Education>
{
    public void Configure(EntityTypeBuilder<Education> builder)
    {
        builder.ToTable("Educations");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.CandidateCvContentId).IsRequired();

        builder.Property(e => e.EducationLevelId).IsRequired();
        builder.Property(e => e.StartDate).IsRequired();

        builder.Property(e => e.CompletionStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.EndDate);
        builder.Property(e => e.DiplomaGradingSystemId);
        builder.Property(e => e.DiplomaGrade).HasColumnType("decimal(5,2)");
        builder.Property(e => e.SchoolId);
        builder.Property(e => e.SchoolNameFreeText).HasMaxLength(300);
        builder.Property(e => e.ProvinceId);
        builder.Property(e => e.Description).HasMaxLength(2000);
    }
}
