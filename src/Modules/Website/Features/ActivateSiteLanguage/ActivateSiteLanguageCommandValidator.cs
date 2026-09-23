using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.ActivateSiteLanguage;

public sealed class ActivateSiteLanguageCommandValidator : AbstractValidator<ActivateSiteLanguageCommand>
{
    public ActivateSiteLanguageCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}
