using GenclikMerkezi.Modules.Employer.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Employer.Infrastructure.Configurations;

public sealed class PersonnelNeedDrivingLicensePreferenceConfiguration : IEntityTypeConfiguration<PersonnelNeedDrivingLicensePreference>
{
    public void Configure(EntityTypeBuilder<PersonnelNeedDrivingLicensePreference> builder)
    {
        builder.ToTable("PersonnelNeedDrivingLicensePreferences");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.PersonnelNeedId).IsRequired();
        builder.Property(p => p.DriversLicenseTypeId).IsRequired();
    }
}
