using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.CreateImpactMetric;

public sealed class CreateImpactMetricCommandValidator : AbstractValidator<CreateImpactMetricCommand>
{
    public CreateImpactMetricCommandValidator()
    {
        RuleFor(c => c.Value).GreaterThanOrEqualTo(0);
        RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);
        RuleFor(c => c.DefaultLanguageLabel).NotEmpty();
    }
}
