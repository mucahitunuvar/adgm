using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class LegalDocumentConfiguration : IEntityTypeConfiguration<LegalDocument>
{
    public void Configure(EntityTypeBuilder<LegalDocument> builder)
    {
        builder.ToTable("LegalDocuments");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.Key)
            .HasConversion(key => key.Value, value => Domain.LegalDocumentKey.Create(value).Value)
            .HasColumnName("Key")
            .HasMaxLength(LegalDocumentKey.MaxLength)
            .IsRequired();
        builder.HasIndex(d => d.Key).IsUnique();

        builder.Property(d => d.Kind).HasConversion<string>().HasMaxLength(30).IsRequired();

        builder.Property(d => d.RowVersion).IsConcurrencyToken();

        builder.Property(d => d.CreatedByUserId).IsRequired();
        builder.Property(d => d.CreatedAtUtc).IsRequired();
        builder.Property(d => d.UpdatedByUserId);
        builder.Property(d => d.UpdatedAtUtc);

        builder.OwnsMany(d => d.Translations, translation =>
        {
            translation.ToTable("LegalDocumentTranslations");
            translation.WithOwner().HasForeignKey("LegalDocumentId");
            translation.HasKey(t => t.Id);
            translation.Property(t => t.Id).ValueGeneratedNever();

            translation.Property(t => t.LanguageCode)
                .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
                .HasColumnName("LanguageCode")
                .HasMaxLength(35)
                .IsRequired();
            translation.HasIndex("LegalDocumentId", nameof(LegalDocumentTranslation.LanguageCode)).IsUnique();

            translation.Property(t => t.Title).HasMaxLength(LegalDocumentTranslation.MaxTitleLength).IsRequired();
        });
        builder.Navigation(d => d.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);

        // ADR-024 §12.1: versions are a whole-aggregate-owned child collection, each with its own
        // further-nested owned translation collection - EF Core supports owned collections nested at
        // any depth, the same shape ContentItem.GalleryItems/Attachments already use for their own
        // per-item translations.
        builder.OwnsMany(d => d.Versions, version =>
        {
            version.ToTable("LegalDocumentVersions");
            version.WithOwner().HasForeignKey("LegalDocumentId");
            version.HasKey(v => v.Id);
            version.Property(v => v.Id).ValueGeneratedNever();

            version.Property(v => v.VersionNumber).IsRequired();
            version.HasIndex("LegalDocumentId", nameof(LegalDocumentVersion.VersionNumber)).IsUnique();

            version.Property(v => v.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            version.Property(v => v.EffectiveAtUtc);
            version.Property(v => v.PublishedAtUtc);
            version.Property(v => v.PublishedByUserId);
            version.Property(v => v.ChangeSummary).HasMaxLength(LegalDocumentVersion.MaxChangeSummaryLength);

            version.OwnsMany(v => v.Translations, translation =>
            {
                translation.ToTable("LegalDocumentVersionTranslations");
                translation.WithOwner().HasForeignKey("LegalDocumentVersionId");
                translation.HasKey(t => t.Id);
                translation.Property(t => t.Id).ValueGeneratedNever();

                translation.Property(t => t.LanguageCode)
                    .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
                    .HasColumnName("LanguageCode")
                    .HasMaxLength(35)
                    .IsRequired();
                translation.HasIndex("LegalDocumentVersionId", nameof(LegalDocumentVersionTranslation.LanguageCode)).IsUnique();

                translation.Property(t => t.Body).IsRequired();
            });
            version.Navigation(v => v.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);
        });
        builder.Navigation(d => d.Versions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
