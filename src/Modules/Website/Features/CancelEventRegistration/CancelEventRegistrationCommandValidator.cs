using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.CancelEventRegistration;

public sealed class CancelEventRegistrationCommandValidator : AbstractValidator<CancelEventRegistrationCommand>
{
    public CancelEventRegistrationCommandValidator()
    {
        RuleFor(c => c.Token).NotEmpty();
    }
}
