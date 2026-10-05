using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.DeactivateFormDefinition;

public sealed class DeactivateFormDefinitionCommandValidator : AbstractValidator<DeactivateFormDefinitionCommand>
{
    public DeactivateFormDefinitionCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
