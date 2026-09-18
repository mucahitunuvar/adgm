using FluentValidation;
using GenclikMerkezi.Modules.Identity.Domain;

namespace GenclikMerkezi.Modules.Identity.Features.CreateStaffUser;

// Deliberately separate from RegisterUserCommandValidator's SelfRegistrableRoles: this command is
// never reachable from the anonymous /api/v1/auth/register endpoint, only from admin-triggered
// module flows (e.g. CareerAdvisor's CreateCareerAdvisorCommand via IIdentityService), so it is
// allowed to grant roles self-registration must never grant.
public sealed class CreateStaffUserCommandValidator : AbstractValidator<CreateStaffUserCommand>
{
    private static readonly string[] StaffRegistrableRoles =
    [
        nameof(UserRole.CareerAdvisor),
    ];

    public CreateStaffUserCommandValidator()
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
            .Must(role => StaffRegistrableRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Role must be a valid staff role.");
    }
}
