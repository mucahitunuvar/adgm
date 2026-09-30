using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    // Seed rows ship with the migration itself, before any real admin user exists - same "system"
    // marker SiteLanguageConfiguration's seed rows use.
    private static readonly Guid SeedUserId = Guid.Empty;
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.ToTable("Menus");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Location).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(m => m.Location).IsUnique();

        builder.Property(m => m.RowVersion).IsConcurrencyToken();

        builder.Property(m => m.CreatedByUserId).IsRequired();
        builder.Property(m => m.CreatedAtUtc).IsRequired();
        builder.Property(m => m.UpdatedByUserId);
        builder.Property(m => m.UpdatedAtUtc);

        // ADR-024 §7 / Faz 2 Görev 1 master prompt §1.2: ParentId is a plain scalar reference to
        // another MenuItem's Id within the same Menu, not an EF navigation/FK - the same
        // self-referencing-hierarchy-as-a-column choice ContentCategory.ParentId already made. The
        // tree itself (cycles, depth, single-level rule) is validated by Menu.ReplaceItems, not the
        // database.
        builder.OwnsMany(m => m.Items, item =>
        {
            item.ToTable("MenuItems");
            item.WithOwner().HasForeignKey("MenuId");
            item.HasKey(i => i.Id);
            item.Property(i => i.Id).ValueGeneratedNever();

            item.Property(i => i.ParentId);
            item.HasIndex("MenuId", nameof(MenuItem.ParentId));

            item.Property(i => i.SortOrder).IsRequired();
            item.Property(i => i.IsActive).IsRequired();
            item.Property(i => i.OpenInNewTab).IsRequired();
            item.Property(i => i.IconKey).HasMaxLength(MenuItem.MaxIconKeyLength);

            item.OwnsOne(i => i.LinkTarget, link =>
            {
                link.ToTable("MenuItems");
                link.Property(l => l.Kind).HasColumnName("LinkKind").HasConversion<string>().HasMaxLength(30).IsRequired();
                link.Property(l => l.ContentItemId).HasColumnName("LinkContentItemId");
                link.Property(l => l.ContentTypeId).HasColumnName("LinkContentTypeId");
                link.Property(l => l.InternalPath).HasColumnName("LinkInternalPath").HasMaxLength(LinkTarget.MaxInternalPathLength);
                link.Property(l => l.ExternalUrl).HasColumnName("LinkExternalUrl").HasMaxLength(LinkTarget.MaxExternalUrlLength);
            });

            item.OwnsMany(i => i.Translations, translation =>
            {
                translation.ToTable("MenuItemTranslations");
                translation.WithOwner().HasForeignKey("MenuItemId");
                translation.HasKey(t => t.Id);
                translation.Property(t => t.Id).ValueGeneratedNever();

                translation.Property(t => t.LanguageCode)
                    .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
                    .HasColumnName("LanguageCode")
                    .HasMaxLength(35)
                    .IsRequired();
                translation.HasIndex("MenuItemId", nameof(MenuItemTranslation.LanguageCode)).IsUnique();

                translation.Property(t => t.Label).HasMaxLength(MenuItemTranslation.MaxLabelLength).IsRequired();
            });
            item.Navigation(i => i.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);
        });
        builder.Navigation(m => m.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        // ADR-024 §7: the three locations ship empty - an admin populates them through
        // ReplaceMenuItems (PUT .../menus/{location}), never through a create/delete endpoint.
        builder.HasData(
            new
            {
                Id = DeterministicGuid.Create("Menu:Header"),
                Location = MenuLocation.Header,
                RowVersion = DeterministicGuid.Create("Menu:Header:RowVersion").ToByteArray(),
                CreatedByUserId = SeedUserId,
                CreatedAtUtc = SeedTimestamp,
            },
            new
            {
                Id = DeterministicGuid.Create("Menu:Utility"),
                Location = MenuLocation.Utility,
                RowVersion = DeterministicGuid.Create("Menu:Utility:RowVersion").ToByteArray(),
                CreatedByUserId = SeedUserId,
                CreatedAtUtc = SeedTimestamp,
            },
            new
            {
                Id = DeterministicGuid.Create("Menu:Footer"),
                Location = MenuLocation.Footer,
                RowVersion = DeterministicGuid.Create("Menu:Footer:RowVersion").ToByteArray(),
                CreatedByUserId = SeedUserId,
                CreatedAtUtc = SeedTimestamp,
            });
    }
}
