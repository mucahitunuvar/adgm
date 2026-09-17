using FluentValidation;
using GenclikMerkezi.Modules.Identity.Domain;

namespace GenclikMerkezi.Modules.Identity.Features.RegisterUser;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    private static readonly string[] SelfRegistrableRoles =
    [
        nameof(UserRole.Candidate),
        nameof(UserRole.Employer),
    ];

    public RegisterUserCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(c => c.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Za-z]").WithMessage("Password must contain at least one letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");

        RuleFor(c => c.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(c => c.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(c => c.PhoneNumber)
            .MaximumLength(20)
            .When(c => c.PhoneNumber is not null);

        RuleFor(c => c.Role)
            .NotEmpty()
            .Must(role => SelfRegistrableRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Role must be either Candidate or Employer.");
    }
}
