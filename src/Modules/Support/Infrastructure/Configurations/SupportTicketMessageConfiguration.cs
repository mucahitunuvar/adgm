using GenclikMerkezi.Modules.Support.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Support.Infrastructure.Configurations;

public sealed class SupportTicketMessageConfiguration : IEntityTypeConfiguration<SupportTicketMessage>
{
    public void Configure(EntityTypeBuilder<SupportTicketMessage> builder)
    {
        builder.ToTable("SupportTicketMessages");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.SupportTicketId).IsRequired();
        builder.HasIndex(m => m.SupportTicketId);

        builder.Property(m => m.SenderUserId).IsRequired();
        builder.Property(m => m.SenderRole).HasConversion<string>().HasMaxLength(50).IsRequired();

        builder.Property(m => m.Content).HasMaxLength(4000).IsRequired();

        builder.Property(m => m.CreatedAtUtc).IsRequired();
    }
}
