using FluentValidation;

namespace GenclikMerkezi.Modules.Candidate.Features.ReassignOrphanedCandidates;

public sealed class ReassignOrphanedCandidatesCommandValidator : AbstractValidator<ReassignOrphanedCandidatesCommand>
{
    public ReassignOrphanedCandidatesCommandValidator()
    {
        RuleFor(c => c.CareerAdvisorId).NotEmpty();
    }
}
