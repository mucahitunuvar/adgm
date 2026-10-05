using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.ActivateFormDefinition;

public sealed class ActivateFormDefinitionCommandValidator : AbstractValidator<ActivateFormDefinitionCommand>
{
    public ActivateFormDefinitionCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
