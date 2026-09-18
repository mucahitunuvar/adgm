using FluentValidation;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.DeactivateCareerAdvisor;

public sealed class DeactivateCareerAdvisorCommandValidator : AbstractValidator<DeactivateCareerAdvisorCommand>
{
    public DeactivateCareerAdvisorCommandValidator()
    {
        RuleFor(c => c.CareerAdvisorId).NotEmpty();
    }
}
