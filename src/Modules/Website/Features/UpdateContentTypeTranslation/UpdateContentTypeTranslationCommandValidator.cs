using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentTypeTranslation;

public sealed class UpdateContentTypeTranslationCommandValidator : AbstractValidator<UpdateContentTypeTranslationCommand>
{
    public UpdateContentTypeTranslationCommandValidator()
    {
        RuleFor(c => c.LanguageCode).NotEmpty();
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.Seo).NotNull();
    }
}
