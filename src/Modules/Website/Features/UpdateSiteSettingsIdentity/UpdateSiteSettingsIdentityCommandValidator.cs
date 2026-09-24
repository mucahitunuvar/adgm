using FluentValidation;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsIdentity;

public sealed class UpdateSiteSettingsIdentityCommandValidator : AbstractValidator<UpdateSiteSettingsIdentityCommand>
{
    public UpdateSiteSettingsIdentityCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();

        RuleForEach(c => c.Translations).ChildRules(translation =>
        {
            translation.RuleFor(t => t.LanguageCode).NotEmpty();
            translation.RuleFor(t => t.SiteName).MaximumLength(SiteSettingsTranslation.MaxSiteNameLength);
            translation.RuleFor(t => t.Tagline).MaximumLength(SiteSettingsTranslation.MaxTaglineLength);
            translation.RuleFor(t => t.DefaultMetaTitle).MaximumLength(SiteSettingsTranslation.MaxMetaTitleLength);
            translation.RuleFor(t => t.DefaultMetaDescription).MaximumLength(SiteSettingsTranslation.MaxMetaDescriptionLength);
            translation.RuleFor(t => t.FooterText).MaximumLength(SiteSettingsTranslation.MaxFooterTextLength);
        });
    }
}
