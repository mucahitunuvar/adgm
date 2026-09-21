using GenclikMerkezi.Modules.Employer.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Employer.Infrastructure.Configurations;

public sealed class PersonnelNeedEducationLevelPreferenceConfiguration : IEntityTypeConfiguration<PersonnelNeedEducationLevelPreference>
{
    public void Configure(EntityTypeBuilder<PersonnelNeedEducationLevelPreference> builder)
    {
        builder.ToTable("PersonnelNeedEducationLevelPreferences");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.PersonnelNeedId).IsRequired();
        builder.Property(p => p.EducationLevelId).IsRequired();
    }
}
