using GenclikMerkezi.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Identity.Infrastructure.Configurations;

public sealed class AdminAuditLogEntryConfiguration : IEntityTypeConfiguration<AdminAuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AdminAuditLogEntry> builder)
    {
        builder.ToTable("AdminAuditLog");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.AdminUserId).IsRequired();

        builder.Property(e => e.ActionType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.TargetUserId).IsRequired();

        builder.Property(e => e.OccurredAtUtc).IsRequired();

        builder.Property(e => e.Details).HasMaxLength(4000);

        builder.Property(e => e.CorrelationId).HasMaxLength(200);

        // The admin/audit-log endpoint filters by each of these independently.
        builder.HasIndex(e => e.TargetUserId);
        builder.HasIndex(e => e.ActionType);
        builder.HasIndex(e => e.OccurredAtUtc);
    }
}
