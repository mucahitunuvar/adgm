using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.SetDefaultSiteLanguage;

public sealed class SetDefaultSiteLanguageCommandValidator : AbstractValidator<SetDefaultSiteLanguageCommand>
{
    public SetDefaultSiteLanguageCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}
