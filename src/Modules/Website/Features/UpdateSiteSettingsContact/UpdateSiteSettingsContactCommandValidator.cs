using FluentValidation;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsContact;

public sealed class UpdateSiteSettingsContactCommandValidator : AbstractValidator<UpdateSiteSettingsContactCommand>
{
    public UpdateSiteSettingsContactCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();

        RuleForEach(c => c.SocialLinks).ChildRules(link =>
        {
            link.RuleFor(l => l.Platform).NotEmpty().MaximumLength(SocialLink.MaxPlatformLength);
            link.RuleFor(l => l.Url).NotEmpty().MaximumLength(SocialLink.MaxUrlLength);
        });
    }
}
