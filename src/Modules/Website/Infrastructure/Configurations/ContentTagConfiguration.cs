using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class ContentTagConfiguration : IEntityTypeConfiguration<ContentTag>
{
    public void Configure(EntityTypeBuilder<ContentTag> builder)
    {
        builder.ToTable("ContentTags");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.LanguageCode)
            .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
            .HasColumnName("LanguageCode")
            .HasMaxLength(35)
            .IsRequired();

        builder.Property(t => t.Name).HasMaxLength(ContentTag.MaxNameLength).IsRequired();
        builder.Property(t => t.Slug).HasMaxLength(Slug.MaxLength).IsRequired();
        builder.HasIndex(t => new { t.LanguageCode, t.Slug }).IsUnique();

        builder.Property(t => t.CreatedByUserId).IsRequired();
        builder.Property(t => t.CreatedAtUtc).IsRequired();
        builder.Property(t => t.UpdatedByUserId);
        builder.Property(t => t.UpdatedAtUtc);
    }
}
