using GenclikMerkezi.Modules.Candidate.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure.Configurations;

public sealed class ExperienceConfiguration : IEntityTypeConfiguration<Experience>
{
    public void Configure(EntityTypeBuilder<Experience> builder)
    {
        builder.ToTable("Experiences");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.CandidateCvContentId).IsRequired();

        builder.Property(e => e.CompanyName).HasMaxLength(300).IsRequired();
        builder.Property(e => e.PositionId);
        builder.Property(e => e.StartDate).IsRequired();
        builder.Property(e => e.EndDate);
        builder.Property(e => e.IsCurrentJob).IsRequired();
        builder.Property(e => e.SectorId);
        builder.Property(e => e.WorkFieldId);
        builder.Property(e => e.EmploymentTypeId);
        builder.Property(e => e.CountryId);
        builder.Property(e => e.ProvinceId);
        builder.Property(e => e.JobDescription).HasMaxLength(4000);
    }
}
