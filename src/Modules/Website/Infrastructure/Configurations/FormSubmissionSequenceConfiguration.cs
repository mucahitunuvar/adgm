using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class FormSubmissionSequenceConfiguration : IEntityTypeConfiguration<FormSubmissionSequence>
{
    public void Configure(EntityTypeBuilder<FormSubmissionSequence> builder)
    {
        builder.ToTable("FormSubmissionSequences");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Year).IsRequired();
        builder.HasIndex(s => s.Year).IsUnique();

        builder.Property(s => s.NextValue).IsRequired();
        builder.Property(s => s.RowVersion).IsConcurrencyToken();
    }
}
