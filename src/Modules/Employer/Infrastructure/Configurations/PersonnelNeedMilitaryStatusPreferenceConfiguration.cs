using GenclikMerkezi.Modules.Employer.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Employer.Infrastructure.Configurations;

public sealed class PersonnelNeedMilitaryStatusPreferenceConfiguration : IEntityTypeConfiguration<PersonnelNeedMilitaryStatusPreference>
{
    public void Configure(EntityTypeBuilder<PersonnelNeedMilitaryStatusPreference> builder)
    {
        builder.ToTable("PersonnelNeedMilitaryStatusPreferences");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.PersonnelNeedId).IsRequired();
        builder.Property(p => p.MilitaryStatusId).IsRequired();
    }
}
