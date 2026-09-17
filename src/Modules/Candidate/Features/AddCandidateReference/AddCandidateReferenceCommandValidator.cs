using FluentValidation;

namespace GenclikMerkezi.Modules.Candidate.Features.AddCandidateReference;

public sealed class AddCandidateReferenceCommandValidator : AbstractValidator<AddCandidateReferenceCommand>
{
    public AddCandidateReferenceCommandValidator()
    {
        RuleFor(c => c.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.LastName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Company).MaximumLength(300).When(c => c.Company is not null);
        RuleFor(c => c.Position).MaximumLength(200).When(c => c.Position is not null);
        RuleFor(c => c.Email).EmailAddress().MaximumLength(256).When(c => c.Email is not null);
        RuleFor(c => c.PhoneNumber).MaximumLength(20).When(c => c.PhoneNumber is not null);
    }
}
