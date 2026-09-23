using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.CreateSiteLanguage;

public sealed class CreateSiteLanguageCommandValidator : AbstractValidator<CreateSiteLanguageCommand>
{
    public CreateSiteLanguageCommandValidator()
    {
        RuleFor(c => c.Code).NotEmpty().MaximumLength(35);
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);
    }
}
