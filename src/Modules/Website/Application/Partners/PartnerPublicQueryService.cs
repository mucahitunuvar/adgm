using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;

namespace GenclikMerkezi.Modules.Website.Application.Partners;

// ADR-024 §8.2 (Faz 2 Görev 3): the single Application-layer service both GetPublicPartnersQuery
// (this Görev, for the işbirlikleri page) and the logo-strip block (Görev 5) call - the same
// "service now, consumer later" shape SliderPublicQueryService already established in Görev 2.
public sealed class PartnerPublicQueryService(
    IPartnerRepository partnerRepository, IMediaAssetRepository mediaAssetRepository, IFileStorageService fileStorageService)
{
    public async Task<IReadOnlyList<PublicPartnerResponse>> GetActiveAsync(LanguageCode languageCode, CancellationToken cancellationToken = default)
    {
        var partners = await partnerRepository.SearchPublicAsync(languageCode, cancellationToken);

        var results = new List<PublicPartnerResponse>();
        foreach (var partner in partners)
        {
            // Guaranteed to exist: SearchPublicAsync only returns partners with a translation in
            // languageCode (§1 "çeviri kuralı").
            var translation = partner.Translations.First(t => t.LanguageCode == languageCode);

            var logoAsset = await mediaAssetRepository.GetByIdAsync(partner.LogoMediaId, cancellationToken);
            if (logoAsset is null)
            {
                // PartnerMediaUsageProvider/MediaImageReferenceGuard should make this unreachable - a
                // logo cannot be deleted while a partner still references it - but a missing image
                // leaves nothing sensible to render, so the partner is skipped rather than shown broken.
                continue;
            }

            var logo = await ToLogoResponseAsync(logoAsset, cancellationToken);

            results.Add(new PublicPartnerResponse(
                partner.Id, translation.Name, translation.Description, partner.WebsiteUrl, logo, partner.SortOrder));
        }

        return results;
    }

    private async Task<PublicPartnerLogoResponse> ToLogoResponseAsync(MediaAsset mediaAsset, CancellationToken cancellationToken)
    {
        var originalUrl = await fileStorageService.GetUrlAsync(mediaAsset.Original.FileKey, cancellationToken);
        string? small = null;
        string? medium = null;
        string? large = null;

        foreach (var variant in mediaAsset.Variants)
        {
            var url = await fileStorageService.GetUrlAsync(variant.File.FileKey, cancellationToken);
            if (variant.VariantName == MediaAssetVariantNames.Small)
            {
                small = url;
            }
            else if (variant.VariantName == MediaAssetVariantNames.Medium)
            {
                medium = url;
            }
            else if (variant.VariantName == MediaAssetVariantNames.Large)
            {
                large = url;
            }
        }

        return new PublicPartnerLogoResponse(small, medium, large, originalUrl);
    }
}
