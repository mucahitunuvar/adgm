using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateImpactMetric;

public sealed class UpdateImpactMetricCommandValidator : AbstractValidator<UpdateImpactMetricCommand>
{
    public UpdateImpactMetricCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.Value).GreaterThanOrEqualTo(0);
        RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);
    }
}
