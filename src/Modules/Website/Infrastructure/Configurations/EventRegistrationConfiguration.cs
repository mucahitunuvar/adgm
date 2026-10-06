using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class EventRegistrationConfiguration : IEntityTypeConfiguration<EventRegistration>
{
    public void Configure(EntityTypeBuilder<EventRegistration> builder)
    {
        builder.ToTable("EventRegistrations");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.EventScheduleId).IsRequired();
        builder.Property(r => r.ContentItemId).IsRequired();
        builder.HasIndex(r => r.ContentItemId);

        builder.Property(r => r.FirstName).HasMaxLength(EventRegistration.MaxFirstNameLength).IsRequired();
        builder.Property(r => r.LastName).HasMaxLength(EventRegistration.MaxLastNameLength).IsRequired();
        builder.Property(r => r.Email).HasMaxLength(EventRegistration.MaxEmailLength).IsRequired();
        builder.HasIndex(r => new { r.ContentItemId, r.Email });
        builder.Property(r => r.Phone).HasMaxLength(EventRegistration.MaxPhoneLength);
        builder.Property(r => r.UserId);

        builder.Property(r => r.LanguageCode)
            .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
            .HasColumnName("LanguageCode")
            .HasMaxLength(35)
            .IsRequired();

        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(r => r.Status);

        builder.Property(r => r.AcceptedPrivacyNoticeKey)
            .HasConversion(key => key.Value, value => LegalDocumentKey.Create(value).Value)
            .HasColumnName("AcceptedPrivacyNoticeKey")
            .HasMaxLength(LegalDocumentKey.MaxLength)
            .IsRequired();
        builder.Property(r => r.AcceptedPrivacyNoticeVersion).IsRequired();

        builder.Property(r => r.CreatedAtUtc).IsRequired();
        builder.Property(r => r.VerifiedAtUtc);
        builder.Property(r => r.StatusChangedAtUtc);
        builder.Property(r => r.WaitlistedAtUtc);
        builder.Property(r => r.CancelledAtUtc);
        builder.Property(r => r.CancelledBy).HasConversion<string?>().HasMaxLength(20);

        builder.Property(r => r.VerificationTokenHash).HasMaxLength(EventRegistration.MaxTokenLength);
        builder.HasIndex(r => r.VerificationTokenHash).IsUnique().HasFilter("[VerificationTokenHash] IS NOT NULL");
        builder.Property(r => r.VerificationTokenExpiresAtUtc);
        builder.Property(r => r.LastVerificationEmailSentAtUtc);

        builder.Property(r => r.CancelToken).HasMaxLength(EventRegistration.MaxTokenLength).IsRequired();
        builder.HasIndex(r => r.CancelToken).IsUnique();

        builder.Property(r => r.AnonymizedAtUtc);
        builder.Property(r => r.RowVersion).IsConcurrencyToken();

        builder.OwnsMany(r => r.StatusHistory, history =>
        {
            history.ToTable("EventRegistrationStatusHistory");
            history.WithOwner().HasForeignKey("EventRegistrationId");
            history.HasKey(h => h.Id);
            history.Property(h => h.Id).ValueGeneratedNever();

            history.Property(h => h.PreviousStatus).HasConversion<string?>().HasMaxLength(20);
            history.Property(h => h.NewStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
            history.Property(h => h.ChangedBy).HasMaxLength(100).IsRequired();
            history.Property(h => h.OccurredAtUtc).IsRequired();
        });
        builder.Navigation(r => r.StatusHistory).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
