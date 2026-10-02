namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// §2 "DELETE .../sliders/{id} (Görev 4'te bir sayfa düzeni kullanıyorsa 409; bu görevde silme serbest,
// kontrol portu kurulur: ISliderUsageChecker)": DeleteSliderCommandHandler's guard, same "port now,
// real implementation once the referencing aggregate exists" shape IVideoUsageChecker used in Faz 1b
// Görev 2 before ContentItem.VideoIds existed.
public interface ISliderUsageChecker
{
    Task<IReadOnlyList<SliderUsage>> GetUsagesAsync(Guid sliderId, CancellationToken cancellationToken = default);
}
