using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Interview.Infrastructure.Configurations;

public sealed class InterviewConfiguration : IEntityTypeConfiguration<Domain.Interview>
{
    public void Configure(EntityTypeBuilder<Domain.Interview> builder)
    {
        builder.ToTable("Interviews");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.CandidateCvId).IsRequired();
        builder.HasIndex(i => i.CandidateCvId);

        builder.Property(i => i.CompanyId).IsRequired();
        builder.HasIndex(i => i.CompanyId);

        builder.Property(i => i.OrganizingAdvisorId).IsRequired();
        builder.HasIndex(i => i.OrganizingAdvisorId);

        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(i => i.ScheduledAtUtc);
        builder.Property(i => i.Outcome).HasConversion<string>().HasMaxLength(50);
        builder.Property(i => i.ResultNotes).HasMaxLength(1000);
        builder.Property(i => i.RequestedByRole).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(i => i.CreatedAtUtc).IsRequired();
    }
}
