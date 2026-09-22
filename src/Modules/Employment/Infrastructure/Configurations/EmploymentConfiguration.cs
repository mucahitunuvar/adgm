using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Employment.Infrastructure.Configurations;

public sealed class EmploymentConfiguration : IEntityTypeConfiguration<Domain.Employment>
{
    public void Configure(EntityTypeBuilder<Domain.Employment> builder)
    {
        builder.ToTable("Employments");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.CandidateCvId).IsRequired();
        builder.HasIndex(e => e.CandidateCvId);

        builder.Property(e => e.CompanyId).IsRequired();
        builder.HasIndex(e => e.CompanyId);

        builder.Property(e => e.PositionId).IsRequired();
        builder.Property(e => e.InterviewId);

        builder.Property(e => e.StartDateUtc).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(e => e.EndDateUtc);
        builder.Property(e => e.DepartureReason).HasMaxLength(1000);

        builder.Property(e => e.CreatedByAdvisorId).IsRequired();
        builder.Property(e => e.CreatedAtUtc).IsRequired();
    }
}
