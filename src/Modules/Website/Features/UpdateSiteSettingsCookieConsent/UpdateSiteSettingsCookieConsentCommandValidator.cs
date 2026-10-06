using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsCookieConsent;

public sealed class UpdateSiteSettingsCookieConsentCommandValidator : AbstractValidator<UpdateSiteSettingsCookieConsentCommand>
{
    public UpdateSiteSettingsCookieConsentCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
