using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class ImpactMetricConfiguration : IEntityTypeConfiguration<ImpactMetric>
{
    public void Configure(EntityTypeBuilder<ImpactMetric> builder)
    {
        builder.ToTable("ImpactMetrics");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Value).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(m => m.IconKey).HasMaxLength(ImpactMetric.MaxIconKeyLength);
        builder.Property(m => m.SortOrder).IsRequired();
        builder.Property(m => m.IsActive).IsRequired();

        builder.OwnsMany(m => m.Translations, translation =>
        {
            translation.ToTable("ImpactMetricTranslations");
            translation.WithOwner().HasForeignKey("ImpactMetricId");
            translation.HasKey(t => t.Id);
            translation.Property(t => t.Id).ValueGeneratedNever();

            translation.Property(t => t.LanguageCode)
                .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
                .HasColumnName("LanguageCode")
                .HasMaxLength(35)
                .IsRequired();
            translation.HasIndex("ImpactMetricId", nameof(ImpactMetricTranslation.LanguageCode)).IsUnique();

            translation.Property(t => t.Label).HasMaxLength(ImpactMetricTranslation.MaxLabelLength).IsRequired();
            translation.Property(t => t.Unit).HasMaxLength(ImpactMetricTranslation.MaxUnitLength);
            translation.Property(t => t.Period).HasMaxLength(ImpactMetricTranslation.MaxPeriodLength);
            translation.Property(t => t.Source).HasMaxLength(ImpactMetricTranslation.MaxSourceLength);
        });
        builder.Navigation(m => m.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(m => m.RowVersion).IsConcurrencyToken();

        builder.Property(m => m.CreatedByUserId).IsRequired();
        builder.Property(m => m.CreatedAtUtc).IsRequired();
        builder.Property(m => m.UpdatedByUserId);
        builder.Property(m => m.UpdatedAtUtc);
    }
}
