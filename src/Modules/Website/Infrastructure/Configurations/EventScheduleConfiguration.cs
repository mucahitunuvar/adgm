using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class EventScheduleConfiguration : IEntityTypeConfiguration<EventSchedule>
{
    public void Configure(EntityTypeBuilder<EventSchedule> builder)
    {
        builder.ToTable("EventSchedules");
        builder.HasKey(es => es.Id);
        builder.Property(es => es.Id).ValueGeneratedNever();

        builder.Property(es => es.ContentItemId).IsRequired();
        builder.HasIndex(es => es.ContentItemId).IsUnique();

        builder.Property(es => es.StartsAtUtc).IsRequired();
        builder.Property(es => es.EndsAtUtc).IsRequired();
        builder.Property(es => es.Format).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(es => es.OnlineLink).HasMaxLength(EventSchedule.MaxOnlineLinkLength);
        builder.Property(es => es.Capacity);
        builder.Property(es => es.RegistrationEnabled).IsRequired();
        builder.Property(es => es.RegistrationOpensAtUtc);
        builder.Property(es => es.RegistrationClosesAtUtc);
        builder.Property(es => es.MinAge);
        builder.Property(es => es.MaxAge);
        builder.Property(es => es.AutoConfirm).IsRequired();
        builder.Property(es => es.WaitlistEnabled).IsRequired();
        builder.Property(es => es.IsCancelled).IsRequired();
        builder.Property(es => es.CancelledAtUtc);
        builder.Property(es => es.CancellationReason).HasMaxLength(EventSchedule.MaxCancellationReasonLength);
        builder.Property(es => es.ConfirmedCount).IsRequired();
        builder.Property(es => es.WaitlistedCount).IsRequired();

        builder.Property(es => es.RowVersion).IsConcurrencyToken();

        builder.Property(es => es.CreatedByUserId).IsRequired();
        builder.Property(es => es.CreatedAtUtc).IsRequired();
        builder.Property(es => es.UpdatedByUserId);
        builder.Property(es => es.UpdatedAtUtc);

        builder.OwnsMany(es => es.Translations, translation =>
        {
            translation.ToTable("EventScheduleTranslations");
            translation.WithOwner().HasForeignKey("EventScheduleId");
            translation.HasKey(t => t.Id);
            translation.Property(t => t.Id).ValueGeneratedNever();

            translation.Property(t => t.LanguageCode)
                .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
                .HasColumnName("LanguageCode")
                .HasMaxLength(35)
                .IsRequired();
            translation.HasIndex("EventScheduleId", nameof(EventScheduleTranslation.LanguageCode)).IsUnique();

            translation.Property(t => t.VenueName).HasMaxLength(EventScheduleTranslation.MaxVenueNameLength).IsRequired();
            translation.Property(t => t.VenueAddress).HasMaxLength(EventScheduleTranslation.MaxVenueAddressLength).IsRequired();
            translation.Property(t => t.FeeInfo).HasMaxLength(EventScheduleTranslation.MaxFeeInfoLength).IsRequired();
            translation.Property(t => t.Instructors).HasMaxLength(EventScheduleTranslation.MaxInstructorsLength).IsRequired();
            translation.Property(t => t.ProgramFlow).IsRequired();
            translation.Property(t => t.AccessibilityNote).HasMaxLength(EventScheduleTranslation.MaxAccessibilityNoteLength).IsRequired();
        });
        builder.Navigation(es => es.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
