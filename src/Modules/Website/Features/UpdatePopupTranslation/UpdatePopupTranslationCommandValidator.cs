using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdatePopupTranslation;

public sealed class UpdatePopupTranslationCommandValidator : AbstractValidator<UpdatePopupTranslationCommand>
{
    public UpdatePopupTranslationCommandValidator()
    {
        RuleFor(c => c.LanguageCode).NotEmpty();
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
