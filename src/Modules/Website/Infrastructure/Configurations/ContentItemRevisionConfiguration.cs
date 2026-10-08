using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class ContentItemRevisionConfiguration : IEntityTypeConfiguration<ContentItemRevision>
{
    public void Configure(EntityTypeBuilder<ContentItemRevision> builder)
    {
        builder.ToTable("ContentItemRevisions");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.ContentItemId).IsRequired();
        builder.Property(r => r.RevisionNumber).IsRequired();
        builder.Property(r => r.SavedAtUtc).IsRequired();
        builder.Property(r => r.SavedByUserId).IsRequired();
        builder.Property(r => r.Kind).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.IsPublishedSnapshot).IsRequired();
        builder.Property(r => r.ContentHash).HasMaxLength(ContentItemRevision.MaxContentHashLength).IsRequired();

        // Unbounded text: a content item's full multi-language snapshot (title/summary/body/seo/tags
        // for every language it has) can exceed any fixed length - same reasoning LayoutBlock.
        // SettingsJson/TextsJson already use for arbitrary JSON payloads.
        builder.Property(r => r.SnapshotJson).IsRequired();

        // Same primitive-collection choice as ContentItem.CategoryIds/ContentItemTranslation.TagIds.
        builder.PrimitiveCollection(r => r.ChangedLanguages).HasColumnName("ChangedLanguages");

        builder.HasIndex(r => new { r.ContentItemId, r.RevisionNumber }).IsUnique();
    }
}
