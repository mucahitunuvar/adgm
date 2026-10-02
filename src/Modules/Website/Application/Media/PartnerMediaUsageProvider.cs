using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Media;

// ADR-024 §8.2 (Faz 2 Görev 3): registered into CompositeMediaUsageChecker (like
// VideoMediaUsageProvider/SliderMediaUsageProvider before it) - a MediaAsset currently used as a
// Partner's logo cannot be deleted.
public sealed class PartnerMediaUsageProvider(IPartnerRepository partnerRepository) : IMediaUsageProvider
{
    public async Task<IReadOnlyList<MediaUsage>> GetUsagesAsync(Guid mediaAssetId, CancellationToken cancellationToken = default)
    {
        var partners = await partnerRepository.SearchByLogoMediaIdAsync(mediaAssetId, cancellationToken);

        return partners
            .Select(p => new MediaUsage("partner", p.Id, BuildDescription(p), "/admin/website/partners"))
            .ToList();
    }

    private static string BuildDescription(Partner partner)
    {
        var name = partner.Translations.FirstOrDefault()?.Name;
        return name is { Length: > 0 } ? $"Partner - {name} (logo)" : "Partner - logo";
    }
}
