using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.CreatePopup;

public sealed class CreatePopupCommandValidator : AbstractValidator<CreatePopupCommand>
{
    public CreatePopupCommandValidator()
    {
        RuleFor(c => c.DisplayMode).NotEmpty();
        RuleFor(c => c.DeviceTarget).NotEmpty();
        RuleFor(c => c.Frequency).NotEmpty();
        RuleFor(c => c.Targeting).NotNull();
        RuleFor(c => c.DelaySeconds).InclusiveBetween(0, 60);
        RuleFor(c => c.Priority).InclusiveBetween(0, 100);
        RuleFor(c => c.FrequencyDays).InclusiveBetween(1, 365).When(c => c.FrequencyDays is not null);
    }
}
