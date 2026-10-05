using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class NewsletterSubscriberConfiguration : IEntityTypeConfiguration<NewsletterSubscriber>
{
    public void Configure(EntityTypeBuilder<NewsletterSubscriber> builder)
    {
        builder.ToTable("NewsletterSubscribers");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Email).HasMaxLength(NewsletterSubscriber.MaxEmailLength).IsRequired();
        builder.HasIndex(s => s.Email).IsUnique();

        builder.Property(s => s.LanguageCode)
            .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
            .HasColumnName("LanguageCode")
            .HasMaxLength(35)
            .IsRequired();

        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(s => s.Status);

        builder.Property(s => s.SubscribedAtUtc).IsRequired();
        builder.Property(s => s.ConfirmedAtUtc);
        builder.Property(s => s.UnsubscribedAtUtc);

        builder.Property(s => s.AcceptedPrivacyNoticeKey)
            .HasConversion(key => key.Value, value => LegalDocumentKey.Create(value).Value)
            .HasColumnName("AcceptedPrivacyNoticeKey")
            .HasMaxLength(LegalDocumentKey.MaxLength)
            .IsRequired();
        builder.Property(s => s.AcceptedPrivacyNoticeVersion).IsRequired();

        builder.Property(s => s.UnsubscribeToken).HasMaxLength(100).IsRequired();
        builder.HasIndex(s => s.UnsubscribeToken).IsUnique();

        builder.Property(s => s.ConfirmationEmailLastSentAtUtc);
        builder.Property(s => s.RowVersion).IsConcurrencyToken();
    }
}
