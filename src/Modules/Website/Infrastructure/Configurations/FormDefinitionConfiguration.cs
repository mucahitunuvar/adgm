using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

// ADR-024 §12.2 (Faz 3 Görev 3): FormDefinition and every owned child it carries (Translations,
// ExplicitConsents, Fields -> Options/Translations) are all configured here, the same single-file-per-
// aggregate shape LegalDocumentConfiguration uses for LegalDocument + Translations + Versions +
// VersionTranslations.
public sealed class FormDefinitionConfiguration : IEntityTypeConfiguration<FormDefinition>
{
    public void Configure(EntityTypeBuilder<FormDefinition> builder)
    {
        builder.ToTable("FormDefinitions");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedNever();

        builder.Property(f => f.Key)
            .HasConversion(key => key.Value, value => Domain.FormDefinitionKey.Create(value).Value)
            .HasColumnName("Key")
            .HasMaxLength(FormDefinitionKey.MaxLength)
            .IsRequired();
        builder.HasIndex(f => f.Key).IsUnique();

        builder.Property(f => f.IsActive).IsRequired();
        builder.Property(f => f.RetentionDays).IsRequired();

        builder.PrimitiveCollection(f => f.NotificationEmails).HasColumnName("NotificationEmails");

        builder.Property(f => f.PrivacyNoticeKey)
            .HasConversion(key => key.Value, value => Domain.LegalDocumentKey.Create(value).Value)
            .HasColumnName("PrivacyNoticeKey")
            .HasMaxLength(LegalDocumentKey.MaxLength)
            .IsRequired();

        builder.Property(f => f.DefinitionVersion).IsRequired();
        builder.Property(f => f.RowVersion).IsConcurrencyToken();

        builder.Property(f => f.CreatedByUserId).IsRequired();
        builder.Property(f => f.CreatedAtUtc).IsRequired();
        builder.Property(f => f.UpdatedByUserId);
        builder.Property(f => f.UpdatedAtUtc);

        builder.OwnsMany(f => f.ExplicitConsents, consent =>
        {
            consent.ToTable("FormExplicitConsentRequirements");
            consent.WithOwner().HasForeignKey("FormDefinitionId");
            consent.HasKey(c => c.Id);
            consent.Property(c => c.Id).ValueGeneratedNever();

            consent.Property(c => c.LegalDocumentKey)
                .HasConversion(key => key.Value, value => Domain.LegalDocumentKey.Create(value).Value)
                .HasColumnName("LegalDocumentKey")
                .HasMaxLength(LegalDocumentKey.MaxLength)
                .IsRequired();
            consent.Property(c => c.IsRequired).IsRequired();
        });
        builder.Navigation(f => f.ExplicitConsents).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(f => f.Translations, translation =>
        {
            translation.ToTable("FormDefinitionTranslations");
            translation.WithOwner().HasForeignKey("FormDefinitionId");
            translation.HasKey(t => t.Id);
            translation.Property(t => t.Id).ValueGeneratedNever();

            translation.Property(t => t.LanguageCode)
                .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
                .HasColumnName("LanguageCode")
                .HasMaxLength(35)
                .IsRequired();
            translation.HasIndex("FormDefinitionId", nameof(FormDefinitionTranslation.LanguageCode)).IsUnique();

            translation.Property(t => t.Title).HasMaxLength(FormDefinitionTranslation.MaxTitleLength).IsRequired();
            translation.Property(t => t.Description).IsRequired();
            translation.Property(t => t.SuccessMessage).HasMaxLength(FormDefinitionTranslation.MaxSuccessMessageLength).IsRequired();
            translation.Property(t => t.SubmitButtonLabel).HasMaxLength(FormDefinitionTranslation.MaxSubmitButtonLabelLength).IsRequired();
        });
        builder.Navigation(f => f.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(f => f.Fields, field =>
        {
            field.ToTable("FormFields");
            field.WithOwner().HasForeignKey("FormDefinitionId");
            field.HasKey(fl => fl.Id);
            field.Property(fl => fl.Id).ValueGeneratedNever();

            field.Property(fl => fl.Key).HasMaxLength(FormField.MaxKeyLength).IsRequired();
            field.HasIndex("FormDefinitionId", nameof(FormField.Key)).IsUnique();

            field.Property(fl => fl.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
            field.Property(fl => fl.IsRequired).IsRequired();
            field.Property(fl => fl.SortOrder).IsRequired();
            field.Property(fl => fl.MinLength);
            field.Property(fl => fl.MaxLength);
            field.Property(fl => fl.DateMin);
            field.Property(fl => fl.DateMax);
            field.Property(fl => fl.MaxSizeMb);

            field.PrimitiveCollection(fl => fl.AllowedFileTypes).HasColumnName("AllowedFileTypes");

            field.OwnsMany(fl => fl.Options, option =>
            {
                option.ToTable("FormFieldOptions");
                option.WithOwner().HasForeignKey("FormFieldId");
                option.HasKey(o => o.Id);
                option.Property(o => o.Id).ValueGeneratedNever();

                option.Property(o => o.Key).HasMaxLength(FormFieldOption.MaxKeyLength).IsRequired();

                option.OwnsMany(o => o.Translations, translation =>
                {
                    translation.ToTable("FormFieldOptionTranslations");
                    translation.WithOwner().HasForeignKey("FormFieldOptionId");
                    translation.HasKey(t => t.Id);
                    translation.Property(t => t.Id).ValueGeneratedNever();

                    translation.Property(t => t.LanguageCode)
                        .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
                        .HasColumnName("LanguageCode")
                        .HasMaxLength(35)
                        .IsRequired();
                    translation.HasIndex("FormFieldOptionId", nameof(FormFieldOptionTranslation.LanguageCode)).IsUnique();

                    translation.Property(t => t.Label).HasMaxLength(FormFieldOptionTranslation.MaxLabelLength).IsRequired();
                });
                option.Navigation(o => o.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);
            });
            field.Navigation(fl => fl.Options).UsePropertyAccessMode(PropertyAccessMode.Field);

            field.OwnsMany(fl => fl.Translations, translation =>
            {
                translation.ToTable("FormFieldTranslations");
                translation.WithOwner().HasForeignKey("FormFieldId");
                translation.HasKey(t => t.Id);
                translation.Property(t => t.Id).ValueGeneratedNever();

                translation.Property(t => t.LanguageCode)
                    .HasConversion(code => code.Value, value => LanguageCode.Create(value).Value)
                    .HasColumnName("LanguageCode")
                    .HasMaxLength(35)
                    .IsRequired();
                translation.HasIndex("FormFieldId", nameof(FormFieldTranslation.LanguageCode)).IsUnique();

                translation.Property(t => t.Label).HasMaxLength(FormFieldTranslation.MaxLabelLength).IsRequired();
                translation.Property(t => t.Placeholder).HasMaxLength(FormFieldTranslation.MaxPlaceholderLength);
                translation.Property(t => t.HelpText).HasMaxLength(FormFieldTranslation.MaxHelpTextLength);
            });
            field.Navigation(fl => fl.Translations).UsePropertyAccessMode(PropertyAccessMode.Field);
        });
        builder.Navigation(f => f.Fields).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
