using GenclikMerkezi.Modules.CareerAdvisor.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.CareerAdvisor.Infrastructure.Configurations;

public sealed class CandidateNoteConfiguration : IEntityTypeConfiguration<CandidateNote>
{
    public void Configure(EntityTypeBuilder<CandidateNote> builder)
    {
        builder.ToTable("CandidateNotes");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedNever();

        builder.Property(n => n.CandidateCvId).IsRequired();
        builder.HasIndex(n => n.CandidateCvId);

        builder.Property(n => n.CareerAdvisorId).IsRequired();

        builder.Property(n => n.NoteType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.Content).HasMaxLength(4000).IsRequired();

        builder.Property(n => n.CreatedAtUtc).IsRequired();
    }
}
