using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

public sealed class ImpactStatsBlockTypeDefinition : BlockTypeDefinition<ImpactStatsBlockSettings, ImpactStatsBlockTexts>
{
    public override string Key => "impact-stats";

    public override IReadOnlyList<PageLayoutTargetKind> AllowedTargets => [PageLayoutTargetKind.Home, PageLayoutTargetKind.Content];

    protected override Result ValidateSettings(ImpactStatsBlockSettings settings)
    {
        if (settings.MetricIds.Count > 8)
        {
            return Result.Failure(Error.Validation("impact-stats.TooManyMetrics", "impact-stats can reference at most 8 metrics."));
        }

        if (settings.MetricIds.Distinct().Count() != settings.MetricIds.Count)
        {
            return Result.Failure(Error.Validation("impact-stats.DuplicateMetricId", "A metric id appears more than once."));
        }

        return Result.Success();
    }

    protected override Result ValidateTexts(ImpactStatsBlockSettings settings, ImpactStatsBlockTexts texts) => Result.Success();

    protected override BlockReferenceSet GetReferences(ImpactStatsBlockSettings settings, IReadOnlyList<ImpactStatsBlockTexts> texts) =>
        BlockReferenceSet.Empty with { ImpactMetricIds = settings.MetricIds };
}
