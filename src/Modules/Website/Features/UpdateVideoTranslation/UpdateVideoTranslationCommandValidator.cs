using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateVideoTranslation;

public sealed class UpdateVideoTranslationCommandValidator : AbstractValidator<UpdateVideoTranslationCommand>
{
    public UpdateVideoTranslationCommandValidator()
    {
        RuleFor(c => c.LanguageCode).NotEmpty();
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.Title).NotEmpty();
    }
}
