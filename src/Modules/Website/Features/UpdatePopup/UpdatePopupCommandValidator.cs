using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdatePopup;

public sealed class UpdatePopupCommandValidator : AbstractValidator<UpdatePopupCommand>
{
    public UpdatePopupCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.DisplayMode).NotEmpty();
        RuleFor(c => c.DeviceTarget).NotEmpty();
        RuleFor(c => c.Frequency).NotEmpty();
        RuleFor(c => c.Targeting).NotNull();
        RuleFor(c => c.DelaySeconds).InclusiveBetween(0, 60);
        RuleFor(c => c.Priority).InclusiveBetween(0, 100);
        RuleFor(c => c.FrequencyDays).InclusiveBetween(1, 365).When(c => c.FrequencyDays is not null);
    }
}
