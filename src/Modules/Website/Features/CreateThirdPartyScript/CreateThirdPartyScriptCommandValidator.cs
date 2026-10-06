using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.CreateThirdPartyScript;

public sealed class CreateThirdPartyScriptCommandValidator : AbstractValidator<CreateThirdPartyScriptCommand>
{
    public CreateThirdPartyScriptCommandValidator()
    {
        RuleFor(c => c.Provider).NotEmpty();
        RuleFor(c => c.Category).NotEmpty();
        RuleFor(c => c.Placement).NotEmpty();
        RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);
        RuleFor(c => c.DefaultLanguageName).NotEmpty();
        RuleFor(c => c.DefaultLanguagePurpose).NotEmpty();
    }
}
