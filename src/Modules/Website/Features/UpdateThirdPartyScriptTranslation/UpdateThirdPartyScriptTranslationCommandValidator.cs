using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateThirdPartyScriptTranslation;

public sealed class UpdateThirdPartyScriptTranslationCommandValidator : AbstractValidator<UpdateThirdPartyScriptTranslationCommand>
{
    public UpdateThirdPartyScriptTranslationCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.LanguageCode).NotEmpty();
    }
}
