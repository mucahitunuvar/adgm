using GenclikMerkezi.Modules.Employer.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Employer.Infrastructure.Configurations;

public sealed class JobEducationLevelPreferenceConfiguration : IEntityTypeConfiguration<JobEducationLevelPreference>
{
    public void Configure(EntityTypeBuilder<JobEducationLevelPreference> builder)
    {
        builder.ToTable("JobEducationLevelPreferences");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.JobId).IsRequired();
        builder.Property(p => p.EducationLevelId).IsRequired();
    }
}
