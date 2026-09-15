using GenclikMerkezi.Modules.Notification.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Notification.Infrastructure.Configurations;

public sealed class EmailNotificationConfiguration : IEntityTypeConfiguration<EmailNotification>
{
    public void Configure(EntityTypeBuilder<EmailNotification> builder)
    {
        builder.ToTable("EmailNotifications");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedNever();

        builder.Property(n => n.ToEmail)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(n => n.Subject)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(n => n.Body)
            .IsRequired();

        builder.Property(n => n.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.CreatedAtUtc).IsRequired();

        builder.Property(n => n.SentAtUtc);

        builder.Property(n => n.FailureReason).HasMaxLength(2000);
    }
}
