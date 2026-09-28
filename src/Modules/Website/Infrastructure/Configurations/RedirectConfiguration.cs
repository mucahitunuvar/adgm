using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class RedirectConfiguration : IEntityTypeConfiguration<Redirect>
{
    public void Configure(EntityTypeBuilder<Redirect> builder)
    {
        builder.ToTable("Redirects");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.LanguageCode)
            .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
            .HasColumnName("LanguageCode")
            .HasMaxLength(35)
            .IsRequired();

        builder.Property(r => r.FromPath).HasMaxLength(2048).IsRequired();
        builder.HasIndex(r => new { r.LanguageCode, r.FromPath }).IsUnique();

        builder.Property(r => r.TargetKind).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.TargetContentItemId);
        builder.Property(r => r.TargetPath).HasMaxLength(2048);
        builder.Property(r => r.StatusCode).HasConversion<int>().IsRequired();
        builder.Property(r => r.IsAutomatic).IsRequired();
        builder.Property(r => r.HitCount).IsRequired();
        builder.Property(r => r.LastHitAtUtc);

        builder.Property(r => r.CreatedByUserId).IsRequired();
        builder.Property(r => r.CreatedAtUtc).IsRequired();
    }
}
