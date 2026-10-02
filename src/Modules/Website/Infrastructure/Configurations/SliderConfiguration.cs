using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class SliderConfiguration : IEntityTypeConfiguration<Slider>
{
    public void Configure(EntityTypeBuilder<Slider> builder)
    {
        builder.ToTable("Sliders");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Key)
            .HasConversion(key => key.Value, value => Domain.SliderKey.Create(value).Value)
            .HasColumnName("Key")
            .HasMaxLength(SliderKey.MaxLength)
            .IsRequired();
        builder.HasIndex(s => s.Key).IsUnique();

        builder.Property(s => s.RowVersion).IsConcurrencyToken();

        builder.Property(s => s.CreatedByUserId).IsRequired();
        builder.Property(s => s.CreatedAtUtc).IsRequired();
        builder.Property(s => s.UpdatedByUserId);
        builder.Property(s => s.UpdatedAtUtc);

        builder.OwnsMany(s => s.Translations, translation =>
        {
            translation.ToTable("SliderTranslations");
            translation.WithOwner().HasForeignKey("SliderId");
            translation.HasKey(t => t.Id);
            translation.Property(t => t.Id).ValueGeneratedNever();

            translation.Property(t => t.LanguageCode)
                .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
                .HasColumnName("LanguageCode")
                .HasMaxLength(35)
                .IsRequired();
            translation.HasIndex("SliderId", nameof(SliderTranslation.LanguageCode)).IsUnique();

            translation.Property(t => t.Name).HasMaxLength(SliderTranslation.MaxNameLength).IsRequired();
        });
        builder.Navigation(s => s.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(s => s.Slides, slide =>
        {
            slide.ToTable("Slides");
            slide.WithOwner().HasForeignKey("SliderId");
            slide.HasKey(sl => sl.Id);
            slide.Property(sl => sl.Id).ValueGeneratedNever();

            slide.Property(sl => sl.DesktopImageMediaId).IsRequired();
            slide.Property(sl => sl.MobileImageMediaId);
            slide.Property(sl => sl.SortOrder).IsRequired();
            slide.Property(sl => sl.IsActive).IsRequired();
            slide.Property(sl => sl.PublishAtUtc);
            slide.Property(sl => sl.UnpublishAtUtc);

            slide.OwnsOne(sl => sl.LinkTarget, link =>
            {
                link.ToTable("Slides");
                link.Property(l => l.Kind).HasColumnName("LinkKind").HasConversion<string>().HasMaxLength(30).IsRequired();
                link.Property(l => l.ContentItemId).HasColumnName("LinkContentItemId");
                link.Property(l => l.ContentTypeId).HasColumnName("LinkContentTypeId");
                link.Property(l => l.InternalPath).HasColumnName("LinkInternalPath").HasMaxLength(LinkTarget.MaxInternalPathLength);
                link.Property(l => l.ExternalUrl).HasColumnName("LinkExternalUrl").HasMaxLength(LinkTarget.MaxExternalUrlLength);
            });

            slide.OwnsMany(sl => sl.Translations, translation =>
            {
                translation.ToTable("SlideTranslations");
                translation.WithOwner().HasForeignKey("SlideId");
                translation.HasKey(t => t.Id);
                translation.Property(t => t.Id).ValueGeneratedNever();

                translation.Property(t => t.LanguageCode)
                    .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
                    .HasColumnName("LanguageCode")
                    .HasMaxLength(35)
                    .IsRequired();
                translation.HasIndex("SlideId", nameof(SlideTranslation.LanguageCode)).IsUnique();

                translation.Property(t => t.Eyebrow).HasMaxLength(SlideTranslation.MaxEyebrowLength);
                translation.Property(t => t.Title).HasMaxLength(SlideTranslation.MaxTitleLength).IsRequired();
                translation.Property(t => t.Text).HasMaxLength(SlideTranslation.MaxTextLength);
                translation.Property(t => t.ButtonLabel).HasMaxLength(SlideTranslation.MaxButtonLabelLength);
                translation.Property(t => t.AltTextOverride).HasMaxLength(SlideTranslation.MaxAltTextOverrideLength);
            });
            slide.Navigation(sl => sl.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);
        });
        builder.Navigation(s => s.Slides).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
