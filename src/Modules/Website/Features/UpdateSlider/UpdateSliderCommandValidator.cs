using FluentValidation;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSlider;

public sealed class UpdateSliderCommandValidator : AbstractValidator<UpdateSliderCommand>
{
    public UpdateSliderCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.Translations).NotNull().NotEmpty();

        RuleForEach(c => c.Translations).ChildRules(translation =>
        {
            translation.RuleFor(t => t.LanguageCode).NotEmpty();
            translation.RuleFor(t => t.Name).NotEmpty().MaximumLength(SliderTranslation.MaxNameLength);
        });
    }
}
