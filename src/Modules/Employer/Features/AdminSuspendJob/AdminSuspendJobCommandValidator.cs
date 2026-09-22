using FluentValidation;

namespace GenclikMerkezi.Modules.Employer.Features.AdminSuspendJob;

public sealed class AdminSuspendJobCommandValidator : AbstractValidator<AdminSuspendJobCommand>
{
    public AdminSuspendJobCommandValidator()
    {
        RuleFor(c => c.JobId).NotEmpty();
        RuleFor(c => c.Reason).NotEmpty().MaximumLength(1000);
    }
}
