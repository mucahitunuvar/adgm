using FluentValidation;

namespace GenclikMerkezi.Modules.Identity.Features.ResendVerificationEmail;

public sealed class ResendVerificationEmailCommandValidator : AbstractValidator<ResendVerificationEmailCommand>
{
    public ResendVerificationEmailCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);
    }
}
