using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.VerifyEventRegistration;

public sealed class VerifyEventRegistrationCommandValidator : AbstractValidator<VerifyEventRegistrationCommand>
{
    public VerifyEventRegistrationCommandValidator()
    {
        RuleFor(c => c.Token).NotEmpty();
    }
}
