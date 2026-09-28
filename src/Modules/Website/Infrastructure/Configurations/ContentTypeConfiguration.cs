using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class ContentTypeConfiguration : IEntityTypeConfiguration<ContentType>
{
    public void Configure(EntityTypeBuilder<ContentType> builder)
    {
        builder.ToTable("ContentTypes");
        builder.HasKey(ct => ct.Id);
        builder.Property(ct => ct.Id).ValueGeneratedNever();

        builder.Property(ct => ct.Key)
            .HasConversion(key => key.Value, value => ContentTypeKey.Create(value).Value)
            .HasColumnName("Key")
            .HasMaxLength(ContentTypeKey.MaxLength)
            .IsRequired();
        builder.HasIndex(ct => ct.Key).IsUnique();

        builder.Property(ct => ct.ListTemplate).HasMaxLength(ContentType.MaxTemplateLength).IsRequired();
        builder.Property(ct => ct.DetailTemplate).HasMaxLength(ContentType.MaxTemplateLength).IsRequired();
        builder.Property(ct => ct.SortMode).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(ct => ct.IsActive).IsRequired();
        builder.Property(ct => ct.SortOrder).IsRequired();

        // ADR-024 §4.1: 14 independent scalar flag columns, mapped directly (not via Flags - see that
        // property's remarks on why a complex-typed column does not work with migration HasData).
        builder.Property(ct => ct.SupportsHierarchy).IsRequired();
        builder.Property(ct => ct.SupportsCategories).IsRequired();
        builder.Property(ct => ct.SupportsTags).IsRequired();
        builder.Property(ct => ct.SupportsDetailImage).IsRequired();
        builder.Property(ct => ct.SupportsGallery).IsRequired();
        builder.Property(ct => ct.SupportsVideos).IsRequired();
        builder.Property(ct => ct.SupportsAttachments).IsRequired();
        builder.Property(ct => ct.SupportsEvent).IsRequired();
        builder.Property(ct => ct.SupportsBlockLayout).IsRequired();
        builder.Property(ct => ct.SupportsForm).IsRequired();
        builder.Property(ct => ct.SupportsRelatedContent).IsRequired();
        builder.Property(ct => ct.HasDetailPage).IsRequired();
        builder.Property(ct => ct.HasListingPage).IsRequired();
        builder.Property(ct => ct.IsSearchable).IsRequired();
        builder.Property(ct => ct.RequiresReview).IsRequired();
        builder.Ignore(ct => ct.Flags);

        builder.OwnsMany(ct => ct.Translations, translation =>
        {
            translation.ToTable("ContentTypeTranslations");
            translation.WithOwner().HasForeignKey("ContentTypeId");
            translation.HasKey(t => t.Id);
            translation.Property(t => t.Id).ValueGeneratedNever();

            translation.Property(t => t.LanguageCode)
                .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
                .HasColumnName("LanguageCode")
                .HasMaxLength(35)
                .IsRequired();
            translation.HasIndex("ContentTypeId", nameof(ContentTypeTranslation.LanguageCode)).IsUnique();

            translation.Property(t => t.Name).HasMaxLength(ContentTypeTranslation.MaxNameLength).IsRequired();
            translation.Property(t => t.RoutePrefix).HasMaxLength(Slug.MaxLength).IsRequired();

            // OwnedNavigationBuilder has no ComplexProperty overload (verified via build error), so
            // SeoMetadata nests as a second-level owned type instead, sharing ContentTypeTranslation's
            // own primary key by convention (see GenclikMerkeziContentTypeSeed.ApplySeo for how its
            // seed rows key off that same Id).
            translation.OwnsOne(t => t.Seo, seo =>
            {
                seo.ToTable("ContentTypeTranslations");
                seo.Property(s => s.MetaTitle).HasColumnName("SeoMetaTitle").HasMaxLength(SeoMetadata.MaxMetaTitleLength).IsRequired();
                seo.Property(s => s.MetaDescription).HasColumnName("SeoMetaDescription").HasMaxLength(SeoMetadata.MaxMetaDescriptionLength).IsRequired();
                seo.Property(s => s.MetaKeywords).HasColumnName("SeoMetaKeywords").HasMaxLength(SeoMetadata.MaxMetaKeywordsLength).IsRequired();
                seo.Property(s => s.OgTitle).HasColumnName("SeoOgTitle").HasMaxLength(SeoMetadata.MaxOgTitleLength).IsRequired();
                seo.Property(s => s.OgDescription).HasColumnName("SeoOgDescription").HasMaxLength(SeoMetadata.MaxOgDescriptionLength).IsRequired();
                seo.Property(s => s.OgImageMediaId).HasColumnName("SeoOgImageMediaId");
                seo.Property(s => s.CanonicalUrl).HasColumnName("SeoCanonicalUrl").HasMaxLength(SeoMetadata.MaxCanonicalUrlLength);
                seo.Property(s => s.NoIndex).HasColumnName("SeoNoIndex").IsRequired();

                GenclikMerkeziContentTypeSeed.ApplySeo(seo);
            });

            GenclikMerkeziContentTypeSeed.ApplyTranslations(translation);
        });
        builder.Navigation(ct => ct.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(ct => ct.RowVersion).IsConcurrencyToken();

        builder.Property(ct => ct.CreatedByUserId).IsRequired();
        builder.Property(ct => ct.CreatedAtUtc).IsRequired();
        builder.Property(ct => ct.UpdatedByUserId);
        builder.Property(ct => ct.UpdatedAtUtc);

        GenclikMerkeziContentTypeSeed.ApplyContentTypes(builder);
    }
}
