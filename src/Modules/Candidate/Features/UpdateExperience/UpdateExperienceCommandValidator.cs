using FluentValidation;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateExperience;

public sealed class UpdateExperienceCommandValidator : AbstractValidator<UpdateExperienceCommand>
{
    public UpdateExperienceCommandValidator()
    {
        RuleFor(c => c.CompanyName).NotEmpty().MaximumLength(300);
        RuleFor(c => c.JobDescription).MaximumLength(4000).When(c => c.JobDescription is not null);

        RuleFor(c => c.EndDate)
            .GreaterThanOrEqualTo(c => c.StartDate)
            .When(c => !c.IsCurrentJob && c.EndDate is not null);
    }
}
