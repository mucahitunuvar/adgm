using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class CookieConsentRecordConfiguration : IEntityTypeConfiguration<CookieConsentRecord>
{
    public void Configure(EntityTypeBuilder<CookieConsentRecord> builder)
    {
        builder.ToTable("CookieConsentRecords");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.ConsentId).IsRequired();
        builder.HasIndex(r => r.ConsentId);

        // Mirrors FormFieldConfiguration's own PrimitiveCollection(fl => fl.AllowedFileTypes) - a small
        // fixed enum set stored as a JSON column, read back into memory for GetCookieConsentSummary
        // (see CookieConsentSummaryItem's remarks for why grouping happens there, not in SQL).
        builder.PrimitiveCollection(r => r.Categories).HasColumnName("Categories").IsRequired();

        builder.Property(r => r.PolicyKey)
            .HasConversion(key => key.Value, value => LegalDocumentKey.Create(value).Value)
            .HasColumnName("PolicyKey")
            .HasMaxLength(LegalDocumentKey.MaxLength)
            .IsRequired();
        builder.Property(r => r.PolicyVersion).IsRequired();

        builder.Property(r => r.Action).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(r => r.RecordedAtUtc).IsRequired();
        builder.HasIndex(r => r.RecordedAtUtc);
    }
}
