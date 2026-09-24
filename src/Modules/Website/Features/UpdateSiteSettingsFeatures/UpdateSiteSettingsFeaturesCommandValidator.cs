using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsFeatures;

public sealed class UpdateSiteSettingsFeaturesCommandValidator : AbstractValidator<UpdateSiteSettingsFeaturesCommand>
{
    public UpdateSiteSettingsFeaturesCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
