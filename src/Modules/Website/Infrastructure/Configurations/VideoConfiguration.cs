using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class VideoConfiguration : IEntityTypeConfiguration<Video>
{
    public void Configure(EntityTypeBuilder<Video> builder)
    {
        builder.ToTable("Videos");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();

        builder.Property(v => v.YouTubeVideoId)
            .HasConversion(id => id.Value, value => Domain.YouTubeVideoId.FromStoredId(value))
            .HasColumnName("YouTubeVideoId")
            .HasMaxLength(Domain.YouTubeVideoId.Length)
            .IsRequired();

        builder.Property(v => v.CoverImageMediaId);
        builder.Property(v => v.SortOrder).IsRequired();
        builder.Property(v => v.IsActive).IsRequired();

        builder.OwnsMany(v => v.Translations, translation =>
        {
            translation.ToTable("VideoTranslations");
            translation.WithOwner().HasForeignKey("VideoId");
            translation.HasKey(t => t.Id);
            translation.Property(t => t.Id).ValueGeneratedNever();

            translation.Property(t => t.LanguageCode)
                .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
                .HasColumnName("LanguageCode")
                .HasMaxLength(35)
                .IsRequired();
            translation.HasIndex("VideoId", nameof(VideoTranslation.LanguageCode)).IsUnique();

            translation.Property(t => t.Title).HasMaxLength(VideoTranslation.MaxTitleLength).IsRequired();
            translation.Property(t => t.Description).HasMaxLength(VideoTranslation.MaxDescriptionLength);
        });
        builder.Navigation(v => v.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(v => v.RowVersion).IsConcurrencyToken();

        builder.Property(v => v.CreatedByUserId).IsRequired();
        builder.Property(v => v.CreatedAtUtc).IsRequired();
        builder.Property(v => v.UpdatedByUserId);
        builder.Property(v => v.UpdatedAtUtc);
    }
}
