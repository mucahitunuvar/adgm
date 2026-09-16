using FluentValidation;

namespace GenclikMerkezi.Modules.Identity.Features.VerifyEmail;

public sealed class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(c => c.Token)
            .NotEmpty();
    }
}
