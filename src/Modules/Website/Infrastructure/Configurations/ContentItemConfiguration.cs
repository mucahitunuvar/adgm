using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class ContentItemConfiguration : IEntityTypeConfiguration<ContentItem>
{
    public void Configure(EntityTypeBuilder<ContentItem> builder)
    {
        builder.ToTable("ContentItems");
        builder.HasKey(ci => ci.Id);
        builder.Property(ci => ci.Id).ValueGeneratedNever();

        builder.Property(ci => ci.ContentTypeId).IsRequired();
        builder.HasIndex(ci => ci.ContentTypeId);

        builder.Property(ci => ci.ParentId);
        builder.Property(ci => ci.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(ci => ci.PublishAtUtc);
        builder.Property(ci => ci.UnpublishAtUtc);
        builder.Property(ci => ci.SortOrder).IsRequired();
        builder.Property(ci => ci.IsFeatured).IsRequired();
        builder.Property(ci => ci.CoverImageMediaId);
        builder.Property(ci => ci.DetailImageMediaId);
        builder.Property(ci => ci.FormDefinitionId);

        // ADR-024 §4.1 (Faz 1b Görev 3): a primitive collection (EF Core 8+), stored as a JSON array
        // column - categories are assigned as a small (<= 10), language-independent set, not a
        // relation queried on its own, so a dedicated join table would be more machinery than the
        // access pattern needs.
        builder.PrimitiveCollection(ci => ci.CategoryIds).HasColumnName("CategoryIds");

        // ADR-024 §4.1 (Faz 1b Görev 4): same primitive-collection choice as CategoryIds - order
        // matters here (declared assignment order), which a JSON array naturally preserves.
        builder.PrimitiveCollection(ci => ci.VideoIds).HasColumnName("VideoIds");

        // ADR-024 §4.1 (Faz 1b Görev 5): same primitive-collection choice as VideoIds - order matters
        // (declared assignment order = manual display order), which a JSON array naturally preserves.
        builder.PrimitiveCollection(ci => ci.RelatedContentItemIds).HasColumnName("RelatedContentItemIds");

        builder.Property(ci => ci.RowVersion).IsConcurrencyToken();

        builder.Property(ci => ci.CreatedByUserId).IsRequired();
        builder.Property(ci => ci.CreatedAtUtc).IsRequired();
        builder.Property(ci => ci.UpdatedByUserId);
        builder.Property(ci => ci.UpdatedAtUtc);
        builder.Property(ci => ci.PublishedByUserId);
        builder.Property(ci => ci.PublishedAtUtc);

        // ADR-024 §4.5 (Faz 1b Görev 6): soft delete - the translation rows and their FullPath/Slug
        // are never touched by MoveToTrash, so the existing (LanguageCode, FullPath) unique index keeps
        // blocking a new item from taking a trashed item's address ("adres kilidi") with no change of
        // its own.
        builder.Property(ci => ci.DeletedAtUtc);
        builder.Property(ci => ci.DeletedByUserId);
        builder.Property(ci => ci.StatusBeforeDeletion).HasConversion<string>().HasMaxLength(20);

        builder.OwnsMany(ci => ci.Translations, translation =>
        {
            translation.ToTable("ContentItemTranslations");
            translation.WithOwner().HasForeignKey("ContentItemId");
            translation.HasKey(t => t.Id);
            translation.Property(t => t.Id).ValueGeneratedNever();

            translation.Property(t => t.LanguageCode)
                .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
                .HasColumnName("LanguageCode")
                .HasMaxLength(35)
                .IsRequired();
            translation.HasIndex("ContentItemId", nameof(ContentItemTranslation.LanguageCode)).IsUnique();

            translation.Property(t => t.Title).HasMaxLength(ContentItemTranslation.MaxTitleLength).IsRequired();
            translation.Property(t => t.Slug).HasMaxLength(Slug.MaxLength).IsRequired();
            translation.Property(t => t.FullPath).HasMaxLength(Slug.MaxLength + 1 + ContentType.MaxTemplateLength).IsRequired();
            // (LanguageCode, FullPath) is unique across the whole module, not scoped to one
            // ContentItem (ADR-024 §4.3) - unlike the index above, this one carries no owner FK.
            translation.HasIndex(nameof(ContentItemTranslation.LanguageCode), nameof(ContentItemTranslation.FullPath)).IsUnique();

            translation.Property(t => t.Summary).HasMaxLength(ContentItemTranslation.MaxSummaryLength).IsRequired();
            translation.Property(t => t.Body).IsRequired();

            // Same primitive-collection choice as ContentItem.CategoryIds, per-translation here since
            // tags are per-language (ADR-024 §4.1).
            translation.PrimitiveCollection(t => t.TagIds).HasColumnName("TagIds");

            translation.OwnsOne(t => t.Seo, seo =>
            {
                seo.ToTable("ContentItemTranslations");
                seo.Property(s => s.MetaTitle).HasColumnName("SeoMetaTitle").HasMaxLength(SeoMetadata.MaxMetaTitleLength).IsRequired();
                seo.Property(s => s.MetaDescription).HasColumnName("SeoMetaDescription").HasMaxLength(SeoMetadata.MaxMetaDescriptionLength).IsRequired();
                seo.Property(s => s.MetaKeywords).HasColumnName("SeoMetaKeywords").HasMaxLength(SeoMetadata.MaxMetaKeywordsLength).IsRequired();
                seo.Property(s => s.OgTitle).HasColumnName("SeoOgTitle").HasMaxLength(SeoMetadata.MaxOgTitleLength).IsRequired();
                seo.Property(s => s.OgDescription).HasColumnName("SeoOgDescription").HasMaxLength(SeoMetadata.MaxOgDescriptionLength).IsRequired();
                seo.Property(s => s.OgImageMediaId).HasColumnName("SeoOgImageMediaId");
                seo.Property(s => s.CanonicalUrl).HasColumnName("SeoCanonicalUrl").HasMaxLength(SeoMetadata.MaxCanonicalUrlLength);
                seo.Property(s => s.NoIndex).HasColumnName("SeoNoIndex").IsRequired();
            });
        });
        builder.Navigation(ci => ci.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);

        // ADR-024 §4.1 (Faz 1b Görev 4): gallery items are a whole-list-replaced child collection,
        // same "own table, FK to owner" shape as Translations - each item's per-language overrides
        // are a further nested owned collection (EF Core supports owned collections nested at any
        // depth, each level configured the same way as the level above it).
        builder.OwnsMany(ci => ci.GalleryItems, item =>
        {
            item.ToTable("ContentItemGalleryItems");
            item.WithOwner().HasForeignKey("ContentItemId");
            item.HasKey(i => i.Id);
            item.Property(i => i.Id).ValueGeneratedNever();
            item.Property(i => i.MediaAssetId).IsRequired();
            item.Property(i => i.SortOrder).IsRequired();

            item.OwnsMany(i => i.Translations, translation =>
            {
                translation.ToTable("ContentItemGalleryItemTranslations");
                translation.WithOwner().HasForeignKey("ContentItemGalleryItemId");
                translation.HasKey(t => t.Id);
                translation.Property(t => t.Id).ValueGeneratedNever();

                translation.Property(t => t.LanguageCode)
                    .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
                    .HasColumnName("LanguageCode")
                    .HasMaxLength(35)
                    .IsRequired();
                translation.HasIndex("ContentItemGalleryItemId", nameof(ContentItemGalleryItemTranslation.LanguageCode)).IsUnique();

                translation.Property(t => t.AltTextOverride).HasMaxLength(ContentItemGalleryItemTranslation.MaxAltTextLength);
                translation.Property(t => t.CaptionOverride).HasMaxLength(ContentItemGalleryItemTranslation.MaxCaptionLength);
            });
            item.Navigation(i => i.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);
        });
        builder.Navigation(ci => ci.GalleryItems).UsePropertyAccessMode(PropertyAccessMode.Field);

        // ADR-024 §4.1 (Faz 1b Görev 4): attachments mirror GalleryItems' shape exactly, with a
        // single DisplayNameOverride instead of AltText/Caption.
        builder.OwnsMany(ci => ci.Attachments, attachment =>
        {
            attachment.ToTable("ContentItemAttachments");
            attachment.WithOwner().HasForeignKey("ContentItemId");
            attachment.HasKey(a => a.Id);
            attachment.Property(a => a.Id).ValueGeneratedNever();
            attachment.Property(a => a.MediaAssetId).IsRequired();
            attachment.Property(a => a.SortOrder).IsRequired();

            attachment.OwnsMany(a => a.Translations, translation =>
            {
                translation.ToTable("ContentItemAttachmentTranslations");
                translation.WithOwner().HasForeignKey("ContentItemAttachmentId");
                translation.HasKey(t => t.Id);
                translation.Property(t => t.Id).ValueGeneratedNever();

                translation.Property(t => t.LanguageCode)
                    .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
                    .HasColumnName("LanguageCode")
                    .HasMaxLength(35)
                    .IsRequired();
                translation.HasIndex("ContentItemAttachmentId", nameof(ContentItemAttachmentTranslation.LanguageCode)).IsUnique();

                translation.Property(t => t.DisplayNameOverride).HasMaxLength(ContentItemAttachmentTranslation.MaxDisplayNameLength);
            });
            attachment.Navigation(a => a.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);
        });
        builder.Navigation(ci => ci.Attachments).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
