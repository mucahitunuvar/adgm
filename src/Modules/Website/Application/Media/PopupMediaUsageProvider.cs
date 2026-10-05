using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Media;

// ADR-024 §9 (Faz 2 Görev 6): registered into CompositeMediaUsageChecker (like
// PartnerMediaUsageProvider/SliderMediaUsageProvider before it) - a MediaAsset currently used as a
// Modal popup's image cannot be deleted.
public sealed class PopupMediaUsageProvider(IPopupRepository popupRepository) : IMediaUsageProvider
{
    public async Task<IReadOnlyList<MediaUsage>> GetUsagesAsync(Guid mediaAssetId, CancellationToken cancellationToken = default)
    {
        var popups = await popupRepository.SearchByImageMediaIdAsync(mediaAssetId, cancellationToken);

        return popups
            .Select(p => new MediaUsage("popup", p.Id, BuildDescription(p), "/admin/website/popups"))
            .ToList();
    }

    private static string BuildDescription(Popup popup)
    {
        var title = popup.Translations.FirstOrDefault()?.Title;
        return title is { Length: > 0 } ? $"Pop-up - {title} (görsel)" : "Pop-up - görsel";
    }
}
