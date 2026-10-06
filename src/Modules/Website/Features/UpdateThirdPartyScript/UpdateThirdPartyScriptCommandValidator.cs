using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateThirdPartyScript;

public sealed class UpdateThirdPartyScriptCommandValidator : AbstractValidator<UpdateThirdPartyScriptCommand>
{
    public UpdateThirdPartyScriptCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.Provider).NotEmpty();
        RuleFor(c => c.Category).NotEmpty();
        RuleFor(c => c.Placement).NotEmpty();
        RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);
    }
}
