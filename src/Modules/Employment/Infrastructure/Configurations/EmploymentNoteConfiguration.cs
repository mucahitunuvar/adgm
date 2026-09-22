using GenclikMerkezi.Modules.Employment.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Employment.Infrastructure.Configurations;

public sealed class EmploymentNoteConfiguration : IEntityTypeConfiguration<EmploymentNote>
{
    public void Configure(EntityTypeBuilder<EmploymentNote> builder)
    {
        builder.ToTable("EmploymentNotes");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedNever();

        builder.Property(n => n.EmploymentId).IsRequired();
        builder.HasIndex(n => n.EmploymentId);

        builder.Property(n => n.CareerAdvisorId).IsRequired();

        builder.Property(n => n.Content).HasMaxLength(4000).IsRequired();

        builder.Property(n => n.CreatedAtUtc).IsRequired();
    }
}
