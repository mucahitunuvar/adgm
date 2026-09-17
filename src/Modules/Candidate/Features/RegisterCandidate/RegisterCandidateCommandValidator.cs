using FluentValidation;

namespace GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;

// Basic request-shape checks only. Password complexity and email uniqueness are enforced by
// Identity's own RegisterUserCommand validator/handler when IIdentityService.CreateUserAsync sends
// it internally - duplicating those rules here would just be two copies of the same policy that can
// drift out of sync.
public sealed class RegisterCandidateCommandValidator : AbstractValidator<RegisterCandidateCommand>
{
    public RegisterCandidateCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(c => c.Password)
            .NotEmpty()
            .MinimumLength(8);

        RuleFor(c => c.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(c => c.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(c => c.PhoneNumber)
            .MaximumLength(20)
            .When(c => c.PhoneNumber is not null);
    }
}
