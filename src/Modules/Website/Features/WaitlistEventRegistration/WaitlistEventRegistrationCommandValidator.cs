using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.WaitlistEventRegistration;

public sealed class WaitlistEventRegistrationCommandValidator : AbstractValidator<WaitlistEventRegistrationCommand>
{
    public WaitlistEventRegistrationCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
