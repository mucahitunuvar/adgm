using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("MediaAssets");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Kind).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.OwnsOne(m => m.Original, original =>
        {
            original.ToTable("MediaAssets");
            original.Property(f => f.FileKey).HasColumnName("OriginalFileKey").HasMaxLength(500).IsRequired();
            original.Property(f => f.OriginalFileName).HasColumnName("OriginalFileName").HasMaxLength(260).IsRequired();
            original.Property(f => f.ContentType).HasColumnName("OriginalContentType").HasMaxLength(100).IsRequired();
            original.Property(f => f.SizeInBytes).HasColumnName("OriginalSizeInBytes").IsRequired();
            original.Property(f => f.UploadedAtUtc).HasColumnName("OriginalUploadedAtUtc").IsRequired();
            original.Property(f => f.OwnerEntityType).HasColumnName("OriginalOwnerEntityType").HasMaxLength(100).IsRequired();
            original.Property(f => f.OwnerEntityId).HasColumnName("OriginalOwnerEntityId").IsRequired();
        });

        builder.OwnsMany(m => m.Variants, variant =>
        {
            variant.ToTable("MediaAssetVariants");
            variant.WithOwner().HasForeignKey("MediaAssetId");
            variant.Property<Guid>("Id");
            variant.HasKey("Id");

            variant.Property(v => v.VariantName).HasMaxLength(20).IsRequired();
            variant.Property(v => v.Width).IsRequired();
            variant.Property(v => v.Height).IsRequired();

            variant.OwnsOne(v => v.File, file =>
            {
                file.ToTable("MediaAssetVariants");
                file.Property(f => f.FileKey).HasColumnName("FileKey").HasMaxLength(500).IsRequired();
                file.Property(f => f.OriginalFileName).HasColumnName("OriginalFileName").HasMaxLength(260).IsRequired();
                file.Property(f => f.ContentType).HasColumnName("ContentType").HasMaxLength(100).IsRequired();
                file.Property(f => f.SizeInBytes).HasColumnName("SizeInBytes").IsRequired();
                file.Property(f => f.UploadedAtUtc).HasColumnName("UploadedAtUtc").IsRequired();
                file.Property(f => f.OwnerEntityType).HasColumnName("OwnerEntityType").HasMaxLength(100).IsRequired();
                file.Property(f => f.OwnerEntityId).HasColumnName("OwnerEntityId").IsRequired();
            });
        });
        builder.Navigation(m => m.Variants).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(m => m.Translations, translation =>
        {
            translation.ToTable("MediaAssetTranslations");
            translation.WithOwner().HasForeignKey("MediaAssetId");
            translation.HasKey(t => t.Id);
            translation.Property(t => t.Id).ValueGeneratedNever();

            translation.Property(t => t.LanguageCode)
                .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
                .HasColumnName("LanguageCode")
                .HasMaxLength(35)
                .IsRequired();
            translation.HasIndex("MediaAssetId", nameof(MediaAssetTranslation.LanguageCode)).IsUnique();

            translation.Property(t => t.AltText).HasMaxLength(500).IsRequired();
            translation.Property(t => t.Caption).HasMaxLength(1000).IsRequired();
        });
        builder.Navigation(m => m.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(m => m.Width);
        builder.Property(m => m.Height);

        builder.Property(m => m.Folder)
            .HasConversion(f => f.Value, v => MediaFolder.Create(v).Value)
            .HasColumnName("Folder")
            .HasMaxLength(MediaFolder.MaxLength)
            .IsRequired();

        builder.Property(m => m.Source).HasMaxLength(500).IsRequired();
        builder.Property(m => m.UsagePermissionNote).HasMaxLength(1000).IsRequired();
        builder.Property(m => m.ContainsPersonalData).IsRequired();

        builder.Property(m => m.CreatedByUserId).IsRequired();
        builder.Property(m => m.CreatedAtUtc).IsRequired();
        builder.Property(m => m.UpdatedByUserId);
        builder.Property(m => m.UpdatedAtUtc);
    }
}
