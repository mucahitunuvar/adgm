using GenclikMerkezi.Modules.Website.Application.Abstractions;

namespace GenclikMerkezi.Modules.Website.Application.Media;

// §2: always-empty stub, exactly like VideoUsageChecker's own pre-Görev-4 state - no aggregate
// references a Slider yet (PageLayout's hero-slider block does not exist until Görev 4, which replaces
// this with a real implementation the same way VideoUsageChecker replaced its own stub in Faz 1b Görev
// 4). Until then, DeleteSlider is always allowed.
public sealed class SliderUsageChecker : ISliderUsageChecker
{
    public Task<IReadOnlyList<SliderUsage>> GetUsagesAsync(Guid sliderId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<SliderUsage>>([]);
}
