using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class SearchDocumentConfiguration : IEntityTypeConfiguration<SearchDocument>
{
    public void Configure(EntityTypeBuilder<SearchDocument> builder)
    {
        builder.ToTable("SearchDocuments");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.SourceKey).HasMaxLength(SearchDocument.MaxSourceKeyLength).IsRequired();
        builder.Property(d => d.SourceId).HasMaxLength(SearchDocument.MaxSourceIdLength).IsRequired();

        builder.Property(d => d.LanguageCode)
            .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
            .HasColumnName("LanguageCode")
            .HasMaxLength(35)
            .IsRequired();

        builder.Property(d => d.TypeKey).HasMaxLength(SearchDocument.MaxTypeKeyLength).IsRequired();
        builder.Property(d => d.Title).HasMaxLength(SearchDocument.MaxTitleLength).IsRequired();
        builder.Property(d => d.Summary).HasMaxLength(SearchDocument.MaxSummaryLength).IsRequired();
        builder.Property(d => d.Url).HasMaxLength(SearchDocument.MaxUrlLength).IsRequired();
        builder.Property(d => d.NormalizedText).HasMaxLength(SearchDocument.MaxNormalizedTextLength).IsRequired();
        builder.Property(d => d.PublishedAtUtc).IsRequired();
        builder.Property(d => d.IndexedAtUtc).IsRequired();
        builder.Property(d => d.IncludeInSitemap).IsRequired();

        builder.HasIndex(d => new { d.SourceKey, d.SourceId, d.LanguageCode }).IsUnique();
        builder.HasIndex(d => new { d.LanguageCode, d.TypeKey });
        builder.HasIndex(d => d.PublishedAtUtc);
    }
}
