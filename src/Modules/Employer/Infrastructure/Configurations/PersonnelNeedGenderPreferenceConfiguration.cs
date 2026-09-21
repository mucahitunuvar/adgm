using GenclikMerkezi.Modules.Employer.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Employer.Infrastructure.Configurations;

public sealed class PersonnelNeedGenderPreferenceConfiguration : IEntityTypeConfiguration<PersonnelNeedGenderPreference>
{
    public void Configure(EntityTypeBuilder<PersonnelNeedGenderPreference> builder)
    {
        builder.ToTable("PersonnelNeedGenderPreferences");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.PersonnelNeedId).IsRequired();
        builder.Property(p => p.GenderId).IsRequired();
    }
}
