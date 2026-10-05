using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.ActivatePopup;

public sealed class ActivatePopupCommandValidator : AbstractValidator<ActivatePopupCommand>
{
    public ActivatePopupCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
