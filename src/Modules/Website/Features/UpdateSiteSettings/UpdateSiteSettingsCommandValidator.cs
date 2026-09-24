using FluentValidation;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettings;

public sealed class UpdateSiteSettingsCommandValidator : AbstractValidator<UpdateSiteSettingsCommand>
{
    public UpdateSiteSettingsCommandValidator()
    {
        RuleFor(c => c.Theme).NotNull();
        RuleFor(c => c.Contact).NotNull();
        RuleFor(c => c.FeatureFlags).NotNull();
        RuleFor(c => c.MaintenanceMessage).MaximumLength(1000);

        RuleForEach(c => c.SocialLinks).ChildRules(link =>
        {
            link.RuleFor(l => l.Platform).NotEmpty().MaximumLength(SocialLink.MaxPlatformLength);
            link.RuleFor(l => l.Url).NotEmpty().MaximumLength(SocialLink.MaxUrlLength);
        });

        RuleForEach(c => c.BankAccounts).ChildRules(account =>
        {
            account.RuleFor(a => a.Iban).NotEmpty();
            account.RuleFor(a => a.BankName).NotEmpty().MaximumLength(BankAccount.MaxBankNameLength);
            account.RuleFor(a => a.AccountHolder).NotEmpty().MaximumLength(BankAccount.MaxAccountHolderLength);
            account.RuleFor(a => a.Description).MaximumLength(BankAccount.MaxDescriptionLength);
        });

        RuleForEach(c => c.Translations).ChildRules(translation =>
        {
            translation.RuleFor(t => t.LanguageCode).NotEmpty();
            translation.RuleFor(t => t.SiteName).MaximumLength(SiteSettingsTranslation.MaxSiteNameLength);
        });
    }
}
