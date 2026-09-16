using GenclikMerkezi.Modules.ReferenceData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

// EF Core's own typed configuration mechanism (IEntityTypeConfiguration<T>) applied generically -
// not a repository abstraction, so this is not the pattern ADR-009 forbids. Each concrete lookup
// still gets its own file/table (below); this only avoids re-typing the shared Code/DisplayName/
// IsActive/SortOrder mapping in each of them.
public abstract class LookupItemConfiguration<TLookup> : IEntityTypeConfiguration<TLookup>
    where TLookup : LookupItem
{
    protected abstract string TableName { get; }

    public virtual void Configure(EntityTypeBuilder<TLookup> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(l => l.Code).IsUnique();

        builder.Property(l => l.DisplayName).HasMaxLength(200).IsRequired();

        builder.Property(l => l.IsActive).IsRequired();

        builder.Property(l => l.SortOrder).IsRequired();
    }
}
