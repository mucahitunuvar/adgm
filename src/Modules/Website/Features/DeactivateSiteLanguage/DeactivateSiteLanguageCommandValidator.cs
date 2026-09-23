using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.DeactivateSiteLanguage;

public sealed class DeactivateSiteLanguageCommandValidator : AbstractValidator<DeactivateSiteLanguageCommand>
{
    public DeactivateSiteLanguageCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}
