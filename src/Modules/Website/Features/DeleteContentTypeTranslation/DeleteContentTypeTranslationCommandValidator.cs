using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.DeleteContentTypeTranslation;

public sealed class DeleteContentTypeTranslationCommandValidator : AbstractValidator<DeleteContentTypeTranslationCommand>
{
    public DeleteContentTypeTranslationCommandValidator()
    {
        RuleFor(c => c.LanguageCode).NotEmpty();
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
