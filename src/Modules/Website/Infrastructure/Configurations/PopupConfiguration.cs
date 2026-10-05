using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class PopupConfiguration : IEntityTypeConfiguration<Popup>
{
    public void Configure(EntityTypeBuilder<Popup> builder)
    {
        builder.ToTable("Popups");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.DisplayMode).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.ImageMediaId);
        builder.Property(p => p.DeviceTarget).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.PublishAtUtc);
        builder.Property(p => p.UnpublishAtUtc);
        builder.Property(p => p.IsActive).IsRequired();
        builder.Property(p => p.DelaySeconds).IsRequired();
        builder.Property(p => p.Frequency).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.FrequencyDays);
        builder.Property(p => p.Dismissible).IsRequired();
        builder.Property(p => p.Priority).IsRequired();

        builder.OwnsOne(p => p.LinkTarget, link =>
        {
            link.ToTable("Popups");
            link.Property(l => l.Kind).HasColumnName("LinkKind").HasConversion<string>().HasMaxLength(30).IsRequired();
            link.Property(l => l.ContentItemId).HasColumnName("LinkContentItemId");
            link.Property(l => l.ContentTypeId).HasColumnName("LinkContentTypeId");
            link.Property(l => l.InternalPath).HasColumnName("LinkInternalPath").HasMaxLength(LinkTarget.MaxInternalPathLength);
            link.Property(l => l.ExternalUrl).HasColumnName("LinkExternalUrl").HasMaxLength(LinkTarget.MaxExternalUrlLength);
        });
        builder.Navigation(p => p.LinkTarget).IsRequired();

        builder.OwnsOne(p => p.Targeting, targeting =>
        {
            targeting.ToTable("Popups");
            targeting.Property(t => t.Kind).HasColumnName("TargetingKind").HasConversion<string>().HasMaxLength(20).IsRequired();
            targeting.PrimitiveCollection(t => t.ContentItemIds).HasColumnName("TargetingContentItemIds");
            targeting.PrimitiveCollection(t => t.Paths).HasColumnName("TargetingPaths");
        });
        builder.Navigation(p => p.Targeting).IsRequired();

        builder.OwnsMany(p => p.Translations, translation =>
        {
            translation.ToTable("PopupTranslations");
            translation.WithOwner().HasForeignKey("PopupId");
            translation.HasKey(t => t.Id);
            translation.Property(t => t.Id).ValueGeneratedNever();

            translation.Property(t => t.LanguageCode)
                .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
                .HasColumnName("LanguageCode")
                .HasMaxLength(35)
                .IsRequired();
            translation.HasIndex("PopupId", nameof(PopupTranslation.LanguageCode)).IsUnique();

            translation.Property(t => t.Title).HasMaxLength(PopupTranslation.MaxTitleLength);
            translation.Property(t => t.Body).IsRequired();
            translation.Property(t => t.ButtonLabel).HasMaxLength(PopupTranslation.MaxButtonLabelLength);
        });
        builder.Navigation(p => p.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(p => p.RowVersion).IsConcurrencyToken();

        builder.Property(p => p.CreatedByUserId).IsRequired();
        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.UpdatedByUserId);
        builder.Property(p => p.UpdatedAtUtc);
    }
}
