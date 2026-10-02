using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.ActivateImpactMetric;

public sealed class ActivateImpactMetricCommandValidator : AbstractValidator<ActivateImpactMetricCommand>
{
    public ActivateImpactMetricCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
