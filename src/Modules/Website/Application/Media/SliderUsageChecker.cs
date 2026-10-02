using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.BlockTypes;

namespace GenclikMerkezi.Modules.Website.Application.Media;

// §4.3: the real implementation, replacing Görev 2's always-empty stub now that PageLayout's
// hero-slider block exists - a Slider referenced by a hero-slider block in any layout's draft or
// published blocks cannot be deleted.
public sealed class SliderUsageChecker(PageLayoutReferenceScanner scanner) : ISliderUsageChecker
{
    public async Task<IReadOnlyList<SliderUsage>> GetUsagesAsync(Guid sliderId, CancellationToken cancellationToken = default)
    {
        var layouts = await scanner.FindReferencingAsync(refs => refs.SliderIds.Contains(sliderId), cancellationToken);

        return layouts
            .Select(layout => new SliderUsage(
                "page-layout", layout.Id, PageLayoutReferenceScanner.DescribeLayout(layout), PageLayoutReferenceScanner.DescribeLayoutUrl(layout)))
            .ToList();
    }
}
