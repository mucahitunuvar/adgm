using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class PageLayoutConfiguration : IEntityTypeConfiguration<PageLayout>
{
    // Same "system" seed marker SiteLanguageConfiguration/MenuConfiguration's seed rows use.
    private static readonly Guid SeedUserId = Guid.Empty;
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<PageLayout> builder)
    {
        builder.ToTable("PageLayouts");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.TargetKind).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.ContentItemId);

        // §4.3 "Content hedefi ... içerik başına tek düzen": the Application layer already checks
        // this (ReplaceContentDraftBlocksCommandHandler calls IPageLayoutRepository.GetByContentItemIdAsync
        // before creating a new row), but a plain (non-filtered) unique index on (TargetKind,
        // ContentItemId) backs it at the database level too - a race between two concurrent first-saves
        // for the same content item would otherwise let both succeed. No filtered/partial index is
        // needed: exactly one row ever has ContentItemId == null (the seeded Home row, never created
        // again through an admin action - IPageLayoutRepository has no "add a Home row" method), so
        // SQL Server's single-NULL-per-unique-index rule is a non-issue here.
        builder.HasIndex(p => new { p.TargetKind, p.ContentItemId }).IsUnique();

        builder.Property(p => p.HasUnpublishedChanges).IsRequired();
        builder.Property(p => p.PublishedAtUtc);
        builder.Property(p => p.PublishedByUserId);

        builder.Property(p => p.RowVersion).IsConcurrencyToken();
        builder.Property(p => p.CreatedByUserId).IsRequired();
        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.UpdatedByUserId);
        builder.Property(p => p.UpdatedAtUtc);

        ConfigureBlocks(builder.OwnsMany(p => p.DraftBlocks), "PageLayoutDraftBlocks", "PageLayoutDraftBlockTranslations");
        builder.Navigation(p => p.DraftBlocks).UsePropertyAccessMode(PropertyAccessMode.Field);

        ConfigureBlocks(builder.OwnsMany(p => p.PublishedBlocks), "PageLayoutPublishedBlocks", "PageLayoutPublishedBlockTranslations");
        builder.Navigation(p => p.PublishedBlocks).UsePropertyAccessMode(PropertyAccessMode.Field);

        // §4.3 "Home için tek bir düzen vardır (migration ile boş seed edilir)" - the same "exactly
        // one row, never created/deleted through an admin action" shape the three Menu rows use.
        builder.HasData(new
        {
            Id = DeterministicGuid.Create("PageLayout:Home"),
            TargetKind = PageLayoutTargetKind.Home,
            ContentItemId = (Guid?)null,
            HasUnpublishedChanges = false,
            PublishedAtUtc = (DateTime?)null,
            PublishedByUserId = (Guid?)null,
            RowVersion = DeterministicGuid.Create("PageLayout:Home:RowVersion").ToByteArray(),
            CreatedByUserId = SeedUserId,
            CreatedAtUtc = SeedTimestamp,
        });
    }

    private static void ConfigureBlocks(OwnedNavigationBuilder<PageLayout, LayoutBlock> block, string tableName, string translationsTableName)
    {
        block.ToTable(tableName);
        block.WithOwner().HasForeignKey("PageLayoutId");
        block.HasKey(b => b.Id);
        block.Property(b => b.Id).ValueGeneratedNever();

        block.Property(b => b.BlockTypeKey).HasMaxLength(50).IsRequired();
        block.Property(b => b.SortOrder).IsRequired();
        block.Property(b => b.IsActive).IsRequired();
        block.Property(b => b.SettingsJson).IsRequired();

        block.OwnsMany(b => b.Translations, translation =>
        {
            translation.ToTable(translationsTableName);
            translation.WithOwner().HasForeignKey("LayoutBlockId");
            translation.HasKey(t => t.Id);
            translation.Property(t => t.Id).ValueGeneratedNever();

            translation.Property(t => t.LanguageCode)
                .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
                .HasColumnName("LanguageCode")
                .HasMaxLength(35)
                .IsRequired();
            translation.HasIndex("LayoutBlockId", nameof(LayoutBlockTranslation.LanguageCode)).IsUnique();

            translation.Property(t => t.TextsJson).IsRequired();
        });
        block.Navigation(b => b.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
