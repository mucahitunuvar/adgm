using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.ConfirmEventRegistration;

public sealed class ConfirmEventRegistrationCommandValidator : AbstractValidator<ConfirmEventRegistrationCommand>
{
    public ConfirmEventRegistrationCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
