using GenclikMerkezi.Modules.Employer.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Employer.Infrastructure.Configurations;

public sealed class JobDrivingLicensePreferenceConfiguration : IEntityTypeConfiguration<JobDrivingLicensePreference>
{
    public void Configure(EntityTypeBuilder<JobDrivingLicensePreference> builder)
    {
        builder.ToTable("JobDrivingLicensePreferences");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.JobId).IsRequired();
        builder.Property(p => p.DriversLicenseTypeId).IsRequired();
    }
}
