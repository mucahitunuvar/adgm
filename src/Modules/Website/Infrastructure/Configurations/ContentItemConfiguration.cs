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

        builder.Property(ci => ci.RowVersion).IsConcurrencyToken();

        builder.Property(ci => ci.CreatedByUserId).IsRequired();
        builder.Property(ci => ci.CreatedAtUtc).IsRequired();
        builder.Property(ci => ci.UpdatedByUserId);
        builder.Property(ci => ci.UpdatedAtUtc);
        builder.Property(ci => ci.PublishedByUserId);
        builder.Property(ci => ci.PublishedAtUtc);

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
    }
}
