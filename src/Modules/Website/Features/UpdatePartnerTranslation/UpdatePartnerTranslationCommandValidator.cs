using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdatePartnerTranslation;

public sealed class UpdatePartnerTranslationCommandValidator : AbstractValidator<UpdatePartnerTranslationCommand>
{
    public UpdatePartnerTranslationCommandValidator()
    {
        RuleFor(c => c.LanguageCode).NotEmpty();
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.Name).NotEmpty();
    }
}
