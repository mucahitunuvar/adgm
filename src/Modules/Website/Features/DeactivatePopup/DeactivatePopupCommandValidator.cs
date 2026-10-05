using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.DeactivatePopup;

public sealed class DeactivatePopupCommandValidator : AbstractValidator<DeactivatePopupCommand>
{
    public DeactivatePopupCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
