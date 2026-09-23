using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteLanguage;

public sealed class UpdateSiteLanguageCommandValidator : AbstractValidator<UpdateSiteLanguageCommand>
{
    public UpdateSiteLanguageCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);
    }
}
