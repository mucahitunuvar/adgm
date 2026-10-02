using FluentValidation;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Features.ReplaceSlides;

public sealed class ReplaceSlidesCommandValidator : AbstractValidator<ReplaceSlidesCommand>
{
    public ReplaceSlidesCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.Slides).NotNull();

        RuleForEach(c => c.Slides).ChildRules(slide =>
        {
            slide.RuleFor(s => s.DesktopImageMediaId).NotEmpty();
            slide.RuleFor(s => s.SortOrder).GreaterThanOrEqualTo(0);
            slide.RuleFor(s => s.Translations).NotNull().NotEmpty();

            slide.RuleForEach(s => s.Translations).ChildRules(translation =>
            {
                translation.RuleFor(t => t.LanguageCode).NotEmpty();
                translation.RuleFor(t => t.Title).NotEmpty().MaximumLength(SlideTranslation.MaxTitleLength);
                translation.RuleFor(t => t.Eyebrow).MaximumLength(SlideTranslation.MaxEyebrowLength);
                translation.RuleFor(t => t.Text).MaximumLength(SlideTranslation.MaxTextLength);
                translation.RuleFor(t => t.ButtonLabel).MaximumLength(SlideTranslation.MaxButtonLabelLength);
                translation.RuleFor(t => t.AltTextOverride).MaximumLength(SlideTranslation.MaxAltTextOverrideLength);
            });

            slide.When(s => s.Link is not null, () =>
            {
                slide.RuleFor(s => s.Link!.Kind).NotEmpty();
            });
        });
    }
}
