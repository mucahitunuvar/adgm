using GenclikMerkezi.Modules.Employer.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Employer.Infrastructure.Configurations;

public sealed class JobLanguageRequirementConfiguration : IEntityTypeConfiguration<JobLanguageRequirement>
{
    public void Configure(EntityTypeBuilder<JobLanguageRequirement> builder)
    {
        builder.ToTable("JobLanguageRequirements");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.JobId).IsRequired();
        builder.Property(r => r.LanguageId).IsRequired();
        builder.Property(r => r.LanguageLevelId).IsRequired();
    }
}
