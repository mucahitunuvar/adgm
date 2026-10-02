using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Media;

// §2 "Medya kullanımı: SliderMediaUsageProvider": registered into CompositeMediaUsageChecker - a
// MediaAsset currently used as a slide's desktop or mobile image cannot be deleted.
public sealed class SliderMediaUsageProvider(ISliderRepository sliderRepository) : IMediaUsageProvider
{
    public async Task<IReadOnlyList<MediaUsage>> GetUsagesAsync(Guid mediaAssetId, CancellationToken cancellationToken = default)
    {
        var sliders = await sliderRepository.SearchByImageIdAsync(mediaAssetId, cancellationToken);

        var usages = new List<MediaUsage>();
        foreach (var slider in sliders)
        {
            foreach (var slide in slider.Slides.Where(s => s.DesktopImageMediaId == mediaAssetId || s.MobileImageMediaId == mediaAssetId))
            {
                var role = slide.DesktopImageMediaId == mediaAssetId ? "masaüstü" : "mobil";
                usages.Add(new MediaUsage(
                    "slider", slider.Id, $"Slider - {DisplayName(slider)} ({role} görsel)", "/admin/website/sliders"));
            }
        }

        return usages;
    }

    private static string DisplayName(Slider slider)
    {
        var name = slider.Translations.FirstOrDefault()?.Name;
        return name is { Length: > 0 } ? name : slider.Key.Value;
    }
}
