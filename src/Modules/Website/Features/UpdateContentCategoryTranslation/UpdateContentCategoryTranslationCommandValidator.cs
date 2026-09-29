using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentCategoryTranslation;

public sealed class UpdateContentCategoryTranslationCommandValidator : AbstractValidator<UpdateContentCategoryTranslationCommand>
{
    public UpdateContentCategoryTranslationCommandValidator()
    {
        RuleFor(c => c.LanguageCode).NotEmpty();
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.Seo).NotNull();
    }
}
