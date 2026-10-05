using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateLegalDocumentTranslation;

public sealed class UpdateLegalDocumentTranslationCommandValidator : AbstractValidator<UpdateLegalDocumentTranslationCommand>
{
    public UpdateLegalDocumentTranslationCommandValidator()
    {
        RuleFor(c => c.LanguageCode).NotEmpty();
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.Title).NotEmpty();
    }
}
