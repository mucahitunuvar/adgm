using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsBotProtection;

public sealed class UpdateSiteSettingsBotProtectionCommandValidator : AbstractValidator<UpdateSiteSettingsBotProtectionCommand>
{
    public UpdateSiteSettingsBotProtectionCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.TurnstileSiteKey).MaximumLength(200);
    }
}
