using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class ContentCategoryConfiguration : IEntityTypeConfiguration<ContentCategory>
{
    public void Configure(EntityTypeBuilder<ContentCategory> builder)
    {
        builder.ToTable("ContentCategories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.ContentTypeId).IsRequired();
        builder.HasIndex(c => c.ContentTypeId);

        builder.Property(c => c.ParentId);
        builder.Property(c => c.SortOrder).IsRequired();
        builder.Property(c => c.IsActive).IsRequired();

        builder.Property(c => c.RowVersion).IsConcurrencyToken();

        builder.Property(c => c.CreatedByUserId).IsRequired();
        builder.Property(c => c.CreatedAtUtc).IsRequired();
        builder.Property(c => c.UpdatedByUserId);
        builder.Property(c => c.UpdatedAtUtc);

        builder.OwnsMany(c => c.Translations, translation =>
        {
            translation.ToTable("ContentCategoryTranslations");
            translation.WithOwner().HasForeignKey("ContentCategoryId");
            translation.HasKey(t => t.Id);
            translation.Property(t => t.Id).ValueGeneratedNever();

            translation.Property(t => t.LanguageCode)
                .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
                .HasColumnName("LanguageCode")
                .HasMaxLength(35)
                .IsRequired();
            translation.HasIndex("ContentCategoryId", nameof(ContentCategoryTranslation.LanguageCode)).IsUnique();

            translation.Property(t => t.Name).HasMaxLength(ContentCategoryTranslation.MaxNameLength).IsRequired();
            translation.Property(t => t.Slug).HasMaxLength(Slug.MaxLength).IsRequired();

            translation.OwnsOne(t => t.Seo, seo =>
            {
                seo.ToTable("ContentCategoryTranslations");
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
        builder.Navigation(c => c.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
