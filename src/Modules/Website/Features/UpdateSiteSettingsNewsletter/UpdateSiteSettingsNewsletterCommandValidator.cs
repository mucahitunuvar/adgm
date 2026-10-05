using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsNewsletter;

public sealed class UpdateSiteSettingsNewsletterCommandValidator : AbstractValidator<UpdateSiteSettingsNewsletterCommand>
{
    public UpdateSiteSettingsNewsletterCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
