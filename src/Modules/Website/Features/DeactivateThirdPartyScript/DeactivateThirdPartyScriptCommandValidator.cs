using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.DeactivateThirdPartyScript;

public sealed class DeactivateThirdPartyScriptCommandValidator : AbstractValidator<DeactivateThirdPartyScriptCommand>
{
    public DeactivateThirdPartyScriptCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
