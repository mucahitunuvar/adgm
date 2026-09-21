using GenclikMerkezi.Modules.Employer.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Employer.Infrastructure.Configurations;

public sealed class JobMilitaryStatusPreferenceConfiguration : IEntityTypeConfiguration<JobMilitaryStatusPreference>
{
    public void Configure(EntityTypeBuilder<JobMilitaryStatusPreference> builder)
    {
        builder.ToTable("JobMilitaryStatusPreferences");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.JobId).IsRequired();
        builder.Property(p => p.MilitaryStatusId).IsRequired();
    }
}
