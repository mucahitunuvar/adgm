using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemForm;

public sealed class SetContentItemFormCommandValidator : AbstractValidator<SetContentItemFormCommand>
{
    public SetContentItemFormCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
