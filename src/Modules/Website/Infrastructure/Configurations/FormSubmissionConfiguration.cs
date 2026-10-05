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
        builder.Property(s => s.AssignedToUserId);
        builder.HasIndex(s => s.AssignedToUserId);
        builder.Property(s => s.ClosedAtUtc);
        builder.Property(s => s.ArchiveEligibleSinceUtc);
        builder.Property(s => s.ArchivedAtUtc);
        builder.HasIndex(s => s.ArchivedAtUtc);
        builder.Property(s => s.AnonymizedAtUtc);
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

        builder.OwnsMany(s => s.StatusHistory, history =>
        {
            history.ToTable("FormSubmissionStatusHistory");
            history.WithOwner().HasForeignKey("FormSubmissionId");
            history.HasKey(h => h.Id);
            history.Property(h => h.Id).ValueGeneratedNever();

            history.Property(h => h.FromStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
            history.Property(h => h.ToStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
            history.Property(h => h.ChangedByUserId).IsRequired();
            history.Property(h => h.ChangedAtUtc).IsRequired();
        });
        builder.Navigation(s => s.StatusHistory).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(s => s.InternalNotes, notes =>
        {
            notes.ToTable("FormSubmissionInternalNotes");
            notes.WithOwner().HasForeignKey("FormSubmissionId");
            notes.HasKey(n => n.Id);
            notes.Property(n => n.Id).ValueGeneratedNever();

            notes.Property(n => n.AuthorUserId).IsRequired();
            notes.Property(n => n.Text).HasMaxLength(FormSubmissionInternalNote.MaxTextLength).IsRequired();
            notes.Property(n => n.CreatedAtUtc).IsRequired();
        });
        builder.Navigation(s => s.InternalNotes).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(s => s.PendingFileDeletions, deletions =>
        {
            deletions.ToTable("FormSubmissionPendingFileDeletions");
            deletions.WithOwner().HasForeignKey("FormSubmissionId");
            deletions.HasKey(d => d.Id);
            deletions.Property(d => d.Id).ValueGeneratedNever();

            deletions.Property(d => d.FileKey).HasMaxLength(500).IsRequired();
        });
        builder.Navigation(s => s.PendingFileDeletions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
