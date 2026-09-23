using GenclikMerkezi.Modules.Support.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Support.Infrastructure.Configurations;

public sealed class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
{
    public void Configure(EntityTypeBuilder<SupportTicket> builder)
    {
        builder.ToTable("SupportTickets");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.OpenedByRole).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(t => t.OpenedByUserId).IsRequired();
        builder.HasIndex(t => t.OpenedByUserId);

        builder.Property(t => t.CandidateCvId);
        builder.Property(t => t.CompanyId);

        builder.Property(t => t.Subject).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Priority).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.HasIndex(t => t.Status);

        builder.Property(t => t.AssignedToUserId);

        builder.Property(t => t.OpenedSinceUtc).IsRequired();

        builder.Property(t => t.ClosedAtUtc);
        builder.Property(t => t.ClosedByUserId);
        builder.Property(t => t.ClosedReason).HasMaxLength(1000);

        builder.Property(t => t.CreatedAtUtc).IsRequired();
    }
}
