using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.CreateSlider;

public sealed class CreateSliderCommandValidator : AbstractValidator<CreateSliderCommand>
{
    public CreateSliderCommandValidator()
    {
        RuleFor(c => c.Key).NotEmpty();
        RuleFor(c => c.DefaultLanguageName).NotEmpty();
    }
}
