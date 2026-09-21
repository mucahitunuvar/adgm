using GenclikMerkezi.Modules.Employer.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Employer.Infrastructure.Configurations;

public sealed class JobGenderPreferenceConfiguration : IEntityTypeConfiguration<JobGenderPreference>
{
    public void Configure(EntityTypeBuilder<JobGenderPreference> builder)
    {
        builder.ToTable("JobGenderPreferences");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.JobId).IsRequired();
        builder.Property(p => p.GenderId).IsRequired();
    }
}
