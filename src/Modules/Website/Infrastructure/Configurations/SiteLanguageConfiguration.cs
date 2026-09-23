using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class SiteLanguageConfiguration : IEntityTypeConfiguration<SiteLanguage>
{
    // Seed rows have no real admin user yet (they ship with the migration itself), so audit fields
    // use a fixed "system" marker instead of a real user id.
    private static readonly Guid SeedUserId = Guid.Empty;
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<SiteLanguage> builder)
    {
        builder.ToTable("SiteLanguages");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.Code)
            .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
            .HasColumnName("Code")
            .HasMaxLength(35)
            .IsRequired();
        builder.HasIndex(l => l.Code).IsUnique();

        builder.Property(l => l.Name).HasMaxLength(100).IsRequired();
        builder.Property(l => l.IsDefault).IsRequired();
        builder.Property(l => l.IsActive).IsRequired();
        builder.Property(l => l.SortOrder).IsRequired();
        builder.Property(l => l.CreatedByUserId).IsRequired();
        builder.Property(l => l.CreatedAtUtc).IsRequired();
        builder.Property(l => l.UpdatedByUserId);
        builder.Property(l => l.UpdatedAtUtc);

        // ADR-024 §3 / Görev 2: tr ships default+active, en ships inactive.
        builder.HasData(
            new
            {
                Id = DeterministicGuid.Create("SiteLanguage:tr"),
                Code = LanguageCode.Create("tr").Value,
                Name = "Türkçe",
                IsDefault = true,
                IsActive = true,
                SortOrder = 1,
                CreatedByUserId = SeedUserId,
                CreatedAtUtc = SeedTimestamp,
            },
            new
            {
                Id = DeterministicGuid.Create("SiteLanguage:en"),
                Code = LanguageCode.Create("en").Value,
                Name = "English",
                IsDefault = false,
                IsActive = false,
                SortOrder = 2,
                CreatedByUserId = SeedUserId,
                CreatedAtUtc = SeedTimestamp,
            });
    }
}
