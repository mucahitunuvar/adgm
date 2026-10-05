using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class FormSubmissionConfiguration : IEntityTypeConfiguration<FormSubmission>
{
    public void Configure(EntityTypeBuilder<FormSubmission> builder)
    {
        builder.ToTable("FormSubmissions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.FormDefinitionId).IsRequired();
        builder.HasIndex(s => s.FormDefinitionId);

        builder.Property(s => s.DefinitionVersion).IsRequired();
        builder.Property(s => s.FieldDefinitionsSnapshotJson).IsRequired();

        builder.Property(s => s.ReferenceNumber).HasMaxLength(30).IsRequired();
        builder.HasIndex(s => s.ReferenceNumber).IsUnique();

        builder.Property(s => s.LanguageCode)
            .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
            .HasColumnName("LanguageCode")
            .HasMaxLength(35)
            .IsRequired();

        builder.Property(s => s.SubmittedAtUtc).IsRequired();
        builder.Property(s => s.SubmittedByUserId);
        builder.Property(s => s.SourceContentItemId);
        builder.Property(s => s.ResponsesJson).IsRequired();

        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(s => s.RowVersion).IsConcurrencyToken();

        builder.OwnsMany(s => s.FileAttachments, attachment =>
        {
            attachment.ToTable("FormSubmissionFileAttachments");
            attachment.WithOwner().HasForeignKey("FormSubmissionId");
            attachment.HasKey(a => a.Id);
            attachment.Property(a => a.Id).ValueGeneratedNever();

            attachment.Property(a => a.FieldKey).HasMaxLength(FormField.MaxKeyLength).IsRequired();

            attachment.OwnsOne(a => a.File, file =>
            {
                file.ToTable("FormSubmissionFileAttachments");
                file.Property(f => f.FileKey).HasColumnName("FileKey").HasMaxLength(500).IsRequired();
                file.Property(f => f.OriginalFileName).HasColumnName("OriginalFileName").HasMaxLength(260).IsRequired();
                file.Property(f => f.ContentType).HasColumnName("ContentType").HasMaxLength(100).IsRequired();
                file.Property(f => f.SizeInBytes).HasColumnName("SizeInBytes").IsRequired();
                file.Property(f => f.UploadedAtUtc).HasColumnName("UploadedAtUtc").IsRequired();
                file.Property(f => f.OwnerEntityType).HasColumnName("OwnerEntityType").HasMaxLength(100).IsRequired();
                file.Property(f => f.OwnerEntityId).HasColumnName("OwnerEntityId").IsRequired();
            });
        });
        builder.Navigation(s => s.FileAttachments).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(s => s.AcceptedLegalVersions, accepted =>
        {
            accepted.ToTable("FormSubmissionAcceptedLegalVersions");
            accepted.WithOwner().HasForeignKey("FormSubmissionId");
            accepted.HasKey(a => a.Id);
            accepted.Property(a => a.Id).ValueGeneratedNever();

            accepted.Property(a => a.LegalDocumentKey)
                .HasConversion(key => key.Value, value => Domain.LegalDocumentKey.Create(value).Value)
                .HasColumnName("LegalDocumentKey")
                .HasMaxLength(LegalDocumentKey.MaxLength)
                .IsRequired();
            accepted.Property(a => a.VersionNumber).IsRequired();
            accepted.Property(a => a.IsPrivacyNotice).IsRequired();
        });
        builder.Navigation(s => s.AcceptedLegalVersions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
