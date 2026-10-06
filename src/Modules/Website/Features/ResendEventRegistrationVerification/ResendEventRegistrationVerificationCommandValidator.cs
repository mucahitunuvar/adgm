using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.ResendEventRegistrationVerification;

public sealed class ResendEventRegistrationVerificationCommandValidator
    : AbstractValidator<ResendEventRegistrationVerificationCommand>
{
    public ResendEventRegistrationVerificationCommandValidator()
    {
        RuleFor(c => c.ContentItemId).NotEmpty();
        RuleFor(c => c.Email).NotEmpty();
    }
}
