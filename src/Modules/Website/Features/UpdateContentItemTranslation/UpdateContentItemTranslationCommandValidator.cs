using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentItemTranslation;

public sealed class UpdateContentItemTranslationCommandValidator : AbstractValidator<UpdateContentItemTranslationCommand>
{
    public UpdateContentItemTranslationCommandValidator()
    {
        RuleFor(c => c.LanguageCode).NotEmpty();
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.Title).NotEmpty();
        RuleFor(c => c.Seo).NotNull();
    }
}
