using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.DeleteFormDefinitionTranslation;

public sealed class DeleteFormDefinitionTranslationCommandValidator : AbstractValidator<DeleteFormDefinitionTranslationCommand>
{
    public DeleteFormDefinitionTranslationCommandValidator()
    {
        RuleFor(c => c.LanguageCode).NotEmpty();
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
