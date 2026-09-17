using FluentValidation;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateCvContentSummary;

public sealed class UpdateCandidateCvContentSummaryCommandValidator : AbstractValidator<UpdateCandidateCvContentSummaryCommand>
{
    public UpdateCandidateCvContentSummaryCommandValidator()
    {
        RuleFor(c => c.Summary).MaximumLength(4000).When(c => c.Summary is not null);
        RuleFor(c => c.ComputerSkills).MaximumLength(2000).When(c => c.ComputerSkills is not null);
        RuleFor(c => c.Hobbies).MaximumLength(2000).When(c => c.Hobbies is not null);
    }
}
