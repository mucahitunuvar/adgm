using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsTheme;

public sealed class UpdateSiteSettingsThemeCommandValidator : AbstractValidator<UpdateSiteSettingsThemeCommand>
{
    public UpdateSiteSettingsThemeCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
