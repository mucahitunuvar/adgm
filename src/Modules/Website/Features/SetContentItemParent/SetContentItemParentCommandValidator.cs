using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemParent;

public sealed class SetContentItemParentCommandValidator : AbstractValidator<SetContentItemParentCommand>
{
    public SetContentItemParentCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
