using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateFormDefinitionTranslation;

public sealed class UpdateFormDefinitionTranslationCommandValidator : AbstractValidator<UpdateFormDefinitionTranslationCommand>
{
    public UpdateFormDefinitionTranslationCommandValidator()
    {
        RuleFor(c => c.LanguageCode).NotEmpty();
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.Title).NotEmpty();
        RuleFor(c => c.SuccessMessage).NotEmpty();
        RuleFor(c => c.SubmitButtonLabel).NotEmpty();
    }
}
