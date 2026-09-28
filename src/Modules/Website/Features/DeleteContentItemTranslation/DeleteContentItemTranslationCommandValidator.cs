using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.DeleteContentItemTranslation;

public sealed class DeleteContentItemTranslationCommandValidator : AbstractValidator<DeleteContentItemTranslationCommand>
{
    public DeleteContentItemTranslationCommandValidator()
    {
        RuleFor(c => c.LanguageCode).NotEmpty();
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
