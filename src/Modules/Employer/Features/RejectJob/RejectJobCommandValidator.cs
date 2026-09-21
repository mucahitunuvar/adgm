using FluentValidation;

namespace GenclikMerkezi.Modules.Employer.Features.RejectJob;

public sealed class RejectJobCommandValidator : AbstractValidator<RejectJobCommand>
{
    public RejectJobCommandValidator()
    {
        RuleFor(c => c.JobId).NotEmpty();
        RuleFor(c => c.Reason).NotEmpty().MaximumLength(1000);
    }
}
