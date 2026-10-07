using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class SearchSourceStateConfiguration : IEntityTypeConfiguration<SearchSourceState>
{
    public void Configure(EntityTypeBuilder<SearchSourceState> builder)
    {
        builder.ToTable("SearchSourceStates");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.SourceKey).HasMaxLength(SearchSourceState.MaxSourceKeyLength).IsRequired();
        builder.HasIndex(s => s.SourceKey).IsUnique();

        builder.Property(s => s.LastStartedAtUtc);
        builder.Property(s => s.LastSucceededAtUtc);
        builder.Property(s => s.LastError).HasMaxLength(SearchSourceState.MaxLastErrorLength);
        builder.Property(s => s.DocumentCount).IsRequired();
    }
}
