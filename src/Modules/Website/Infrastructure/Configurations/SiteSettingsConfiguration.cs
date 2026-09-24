using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class SiteSettingsConfiguration : IEntityTypeConfiguration<SiteSettings>
{
    public void Configure(EntityTypeBuilder<SiteSettings> builder)
    {
        builder.ToTable("SiteSettings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.LogoLightMediaAssetId);
        builder.Property(s => s.LogoDarkMediaAssetId);
        builder.Property(s => s.FaviconMediaAssetId);
        builder.Property(s => s.DefaultOgImageMediaId);

        // Application-managed concurrency token (see SiteSettings' class remarks for why this is not
        // a database-generated rowversion/timestamp column, i.e. not .IsRowVersion()): EF still
        // enforces it exactly like one - every UPDATE's WHERE clause checks the value the caller read
        // it with, and a mismatch throws DbUpdateConcurrencyException.
        builder.Property(s => s.RowVersion).IsConcurrencyToken();

        builder.OwnsOne(s => s.Theme, theme =>
        {
            theme.ToTable("SiteSettings");
            theme.Property(t => t.PrimaryColorHex).HasColumnName("ThemePrimaryColorHex").HasMaxLength(7).IsRequired();
            theme.Property(t => t.SecondaryColorHex).HasColumnName("ThemeSecondaryColorHex").HasMaxLength(7).IsRequired();
            theme.Property(t => t.FontFamily).HasColumnName("ThemeFontFamily").HasMaxLength(SiteTheme.MaxFontFamilyLength).IsRequired();
        });

        builder.OwnsOne(s => s.Contact, contact =>
        {
            contact.ToTable("SiteSettings");
            contact.Property(c => c.Address).HasColumnName("ContactAddress").HasMaxLength(ContactInfo.MaxAddressLength).IsRequired();
            contact.Property(c => c.Phone).HasColumnName("ContactPhone").HasMaxLength(ContactInfo.MaxShortFieldLength).IsRequired();
            contact.Property(c => c.Email).HasColumnName("ContactEmail").HasMaxLength(ContactInfo.MaxShortFieldLength).IsRequired();
            contact.Property(c => c.WhatsApp).HasColumnName("ContactWhatsApp").HasMaxLength(ContactInfo.MaxShortFieldLength).IsRequired();
            contact.Property(c => c.MapEmbedUrl).HasColumnName("ContactMapEmbedUrl").HasMaxLength(ContactInfo.MaxUrlLength).IsRequired();
        });

        builder.OwnsMany(s => s.SocialLinks, link =>
        {
            link.ToTable("SiteSettingsSocialLinks");
            link.WithOwner().HasForeignKey("SiteSettingsId");
            link.HasKey(l => l.Id);
            link.Property(l => l.Id).ValueGeneratedNever();
            link.Property(l => l.Platform).HasMaxLength(SocialLink.MaxPlatformLength).IsRequired();
            link.Property(l => l.Url).HasMaxLength(SocialLink.MaxUrlLength).IsRequired();
            link.Property(l => l.SortOrder).IsRequired();
        });
        builder.Navigation(s => s.SocialLinks).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(s => s.BankAccounts, account =>
        {
            account.ToTable("SiteSettingsBankAccounts");
            account.WithOwner().HasForeignKey("SiteSettingsId");
            account.HasKey(a => a.Id);
            account.Property(a => a.Id).ValueGeneratedNever();

            account.Property(a => a.Iban)
                .HasConversion(iban => iban.Value, value => Iban.Create(value).Value)
                .HasColumnName("Iban")
                .HasMaxLength(Iban.MaxLength)
                .IsRequired();

            account.Property(a => a.BankName).HasMaxLength(BankAccount.MaxBankNameLength).IsRequired();
            account.Property(a => a.AccountHolder).HasMaxLength(BankAccount.MaxAccountHolderLength).IsRequired();
            account.Property(a => a.Currency).HasConversion<string>().HasMaxLength(3).IsRequired();
            account.Property(a => a.Description).HasMaxLength(BankAccount.MaxDescriptionLength);
            account.Property(a => a.SortOrder).IsRequired();
            account.Property(a => a.IsActive).IsRequired();
        });
        builder.Navigation(s => s.BankAccounts).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(s => s.Translations, translation =>
        {
            translation.ToTable("SiteSettingsTranslations");
            translation.WithOwner().HasForeignKey("SiteSettingsId");
            translation.HasKey(t => t.Id);
            translation.Property(t => t.Id).ValueGeneratedNever();

            translation.Property(t => t.LanguageCode)
                .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
                .HasColumnName("LanguageCode")
                .HasMaxLength(35)
                .IsRequired();
            translation.HasIndex("SiteSettingsId", nameof(SiteSettingsTranslation.LanguageCode)).IsUnique();

            translation.Property(t => t.SiteName).HasMaxLength(SiteSettingsTranslation.MaxSiteNameLength).IsRequired();
            translation.Property(t => t.Tagline).HasMaxLength(SiteSettingsTranslation.MaxTaglineLength).IsRequired();
            translation.Property(t => t.DefaultMetaTitle).HasMaxLength(SiteSettingsTranslation.MaxMetaTitleLength).IsRequired();
            translation.Property(t => t.DefaultMetaDescription).HasMaxLength(SiteSettingsTranslation.MaxMetaDescriptionLength).IsRequired();
            translation.Property(t => t.FooterText).HasMaxLength(SiteSettingsTranslation.MaxFooterTextLength).IsRequired();
            translation.Property(t => t.MaintenanceMessage).HasMaxLength(SiteSettingsTranslation.MaxMaintenanceMessageLength).IsRequired();
        });
        builder.Navigation(s => s.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(s => s.GlobalSearchEnabled).IsRequired();
        builder.Property(s => s.NewsletterEnabled).IsRequired();
        builder.Property(s => s.PublicJobListingsEnabled).IsRequired();
        builder.Property(s => s.DonationPageEnabled).IsRequired();
        builder.Property(s => s.BotProtectionEnabled).IsRequired();

        builder.Property(s => s.MaintenanceModeEnabled).IsRequired();
        builder.Property(s => s.TurnstileSiteKey).HasMaxLength(200).IsRequired();

        builder.Property(s => s.UpdatedByUserId);
        builder.Property(s => s.UpdatedAtUtc);
    }
}
