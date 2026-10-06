using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.ActivateThirdPartyScript;

public sealed class ActivateThirdPartyScriptCommandValidator : AbstractValidator<ActivateThirdPartyScriptCommand>
{
    public ActivateThirdPartyScriptCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
