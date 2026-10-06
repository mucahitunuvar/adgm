using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsEvents;

public sealed class UpdateSiteSettingsEventsCommandValidator : AbstractValidator<UpdateSiteSettingsEventsCommand>
{
    public UpdateSiteSettingsEventsCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
