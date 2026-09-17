using FluentValidation;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateCvContactInfo;

public sealed class UpdateCandidateCvContactInfoCommandValidator : AbstractValidator<UpdateCandidateCvContactInfoCommand>
{
    public UpdateCandidateCvContactInfoCommandValidator()
    {
        RuleFor(c => c.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.LastName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(c => c.PhoneNumber).MaximumLength(20).When(c => c.PhoneNumber is not null);
        RuleFor(c => c.Address).MaximumLength(500).When(c => c.Address is not null);

        RuleForEach(c => c.SocialMediaLinks).ChildRules(link =>
        {
            link.RuleFor(l => l.Platform).NotEmpty().MaximumLength(100);
            link.RuleFor(l => l.Url).NotEmpty().MaximumLength(500);
        });
    }
}
