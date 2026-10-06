using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class ThirdPartyScriptConfiguration : IEntityTypeConfiguration<ThirdPartyScript>
{
    public void Configure(EntityTypeBuilder<ThirdPartyScript> builder)
    {
        builder.ToTable("ThirdPartyScripts");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        // ADR-024 §13 (Faz 3 Görev 7): Provider is the "one VO, several factories, empty elsewhere"
        // shape - every column below is nullable since only the group matching Provider.Kind is ever
        // populated (mirrors how PopupTargeting's own columns would be configured, if it were owned
        // instead of primitive-collection-backed).
        builder.OwnsOne(s => s.Provider, provider =>
        {
            provider.ToTable("ThirdPartyScripts");
            provider.Property(p => p.Kind).HasColumnName("ProviderKind").HasConversion<string>().HasMaxLength(30).IsRequired();
            provider.Property(p => p.MeasurementId).HasColumnName("MeasurementId").HasMaxLength(20);
            provider.Property(p => p.ContainerId).HasColumnName("ContainerId").HasMaxLength(20);
            provider.Property(p => p.PixelId).HasColumnName("PixelId").HasMaxLength(20);
            provider.Property(p => p.Src).HasColumnName("Src").HasMaxLength(ThirdPartyScriptProvider.MaxSrcLength);
            provider.Property(p => p.Async).HasColumnName("Async").IsRequired();
            provider.Property(p => p.Defer).HasColumnName("Defer").IsRequired();
        });

        builder.Property(s => s.Category).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(s => s.Placement).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(s => s.SortOrder).IsRequired();
        builder.Property(s => s.IsActive).IsRequired();

        builder.OwnsMany(s => s.Translations, translation =>
        {
            translation.ToTable("ThirdPartyScriptTranslations");
            translation.WithOwner().HasForeignKey("ThirdPartyScriptId");
            translation.HasKey(t => t.Id);
            translation.Property(t => t.Id).ValueGeneratedNever();

            translation.Property(t => t.LanguageCode)
                .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
                .HasColumnName("LanguageCode")
                .HasMaxLength(35)
                .IsRequired();
            translation.HasIndex("ThirdPartyScriptId", nameof(ThirdPartyScriptTranslation.LanguageCode)).IsUnique();

            translation.Property(t => t.Name).HasMaxLength(ThirdPartyScriptTranslation.MaxNameLength).IsRequired();
            translation.Property(t => t.Purpose).HasMaxLength(ThirdPartyScriptTranslation.MaxPurposeLength).IsRequired();
        });
        builder.Navigation(s => s.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(s => s.RowVersion).IsConcurrencyToken();

        builder.Property(s => s.CreatedByUserId).IsRequired();
        builder.Property(s => s.CreatedAtUtc).IsRequired();
        builder.Property(s => s.UpdatedByUserId);
        builder.Property(s => s.UpdatedAtUtc);
    }
}
