using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.ToTable("Partners");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.LogoMediaId).IsRequired();
        builder.Property(p => p.WebsiteUrl).HasMaxLength(Partner.MaxWebsiteUrlLength);
        builder.Property(p => p.SortOrder).IsRequired();
        builder.Property(p => p.IsActive).IsRequired();

        builder.OwnsMany(p => p.Translations, translation =>
        {
            translation.ToTable("PartnerTranslations");
            translation.WithOwner().HasForeignKey("PartnerId");
            translation.HasKey(t => t.Id);
            translation.Property(t => t.Id).ValueGeneratedNever();

            translation.Property(t => t.LanguageCode)
                .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
                .HasColumnName("LanguageCode")
                .HasMaxLength(35)
                .IsRequired();
            translation.HasIndex("PartnerId", nameof(PartnerTranslation.LanguageCode)).IsUnique();

            translation.Property(t => t.Name).HasMaxLength(PartnerTranslation.MaxNameLength).IsRequired();
            translation.Property(t => t.Description).HasMaxLength(PartnerTranslation.MaxDescriptionLength);
        });
        builder.Navigation(p => p.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(p => p.RowVersion).IsConcurrencyToken();

        builder.Property(p => p.CreatedByUserId).IsRequired();
        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.UpdatedByUserId);
        builder.Property(p => p.UpdatedAtUtc);
    }
}
