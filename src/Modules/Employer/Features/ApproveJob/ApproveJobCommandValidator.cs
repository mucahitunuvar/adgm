using FluentValidation;

namespace GenclikMerkezi.Modules.Employer.Features.ApproveJob;

public sealed class ApproveJobCommandValidator : AbstractValidator<ApproveJobCommand>
{
    public ApproveJobCommandValidator()
    {
        RuleFor(c => c.JobId).NotEmpty();
    }
}
