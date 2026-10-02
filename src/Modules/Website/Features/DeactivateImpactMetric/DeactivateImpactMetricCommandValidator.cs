using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.DeactivateImpactMetric;

public sealed class DeactivateImpactMetricCommandValidator : AbstractValidator<DeactivateImpactMetricCommand>
{
    public DeactivateImpactMetricCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
