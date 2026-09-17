using FluentValidation;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateCvPersonalInfo;

public sealed class UpdateCandidateCvPersonalInfoCommandValidator : AbstractValidator<UpdateCandidateCvPersonalInfoCommand>
{
    public UpdateCandidateCvPersonalInfoCommandValidator()
    {
        RuleFor(c => c.Title).MaximumLength(200).When(c => c.Title is not null);
        RuleFor(c => c.NetSalaryExpectation).GreaterThanOrEqualTo(0).When(c => c.NetSalaryExpectation is not null);

        RuleFor(c => c.DisabilityInfo!.Description)
            .NotEmpty()
            .MaximumLength(2000)
            .When(c => c.DisabilityInfo is not null);

        RuleFor(c => c.DisabilityInfo!.Percentage)
            .InclusiveBetween(0, 100)
            .When(c => c.DisabilityInfo is not null);
    }
}
