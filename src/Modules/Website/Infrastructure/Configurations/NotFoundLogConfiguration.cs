using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class NotFoundLogConfiguration : IEntityTypeConfiguration<NotFoundLog>
{
    public void Configure(EntityTypeBuilder<NotFoundLog> builder)
    {
        builder.ToTable("NotFoundLogs");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedNever();

        builder.Property(n => n.LanguageCode)
            .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
            .HasColumnName("LanguageCode")
            .HasMaxLength(35)
            .IsRequired();

        builder.Property(n => n.Path).HasMaxLength(NotFoundLog.MaxPathLength).IsRequired();
        builder.HasIndex(n => new { n.LanguageCode, n.Path }).IsUnique();
        builder.HasIndex(n => n.HitCount);

        builder.Property(n => n.HitCount).IsRequired();
        builder.Property(n => n.FirstSeenAtUtc).IsRequired();
        builder.Property(n => n.LastSeenAtUtc).IsRequired();
    }
}
