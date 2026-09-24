using FluentValidation;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsMaintenance;

public sealed class UpdateSiteSettingsMaintenanceCommandValidator : AbstractValidator<UpdateSiteSettingsMaintenanceCommand>
{
    public UpdateSiteSettingsMaintenanceCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();

        RuleForEach(c => c.Translations).ChildRules(translation =>
        {
            translation.RuleFor(t => t.LanguageCode).NotEmpty();
            translation.RuleFor(t => t.MaintenanceMessage).MaximumLength(SiteSettingsTranslation.MaxMaintenanceMessageLength);
        });
    }
}
