using FluentValidation;

namespace GenclikMerkezi.Modules.Candidate.Features.AdminReassignCandidate;

public sealed class AdminReassignCandidateCommandValidator : AbstractValidator<AdminReassignCandidateCommand>
{
    public AdminReassignCandidateCommandValidator()
    {
        RuleFor(c => c.CandidateCvId).NotEmpty();
    }
}
