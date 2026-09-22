using FluentValidation;

namespace GenclikMerkezi.Modules.Employer.Features.AdminReinstateJob;

public sealed class AdminReinstateJobCommandValidator : AbstractValidator<AdminReinstateJobCommand>
{
    public AdminReinstateJobCommandValidator()
    {
        RuleFor(c => c.JobId).NotEmpty();
    }
}
