using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPartnerById;

public sealed class GetPartnerByIdQueryHandler(
    IPartnerRepository partnerRepository, IMediaAssetRepository mediaAssetRepository, IFileStorageService fileStorageService)
    : IRequestHandler<GetPartnerByIdQuery, Result<PartnerDetailResponse>>
{
    public async Task<Result<PartnerDetailResponse>> Handle(GetPartnerByIdQuery request, CancellationToken cancellationToken)
    {
        var partner = await partnerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (partner is null)
        {
            return Result.Failure<PartnerDetailResponse>(Error.NotFound("Partner.NotFound", $"Partner '{request.Id}' could not be found."));
        }

        var logo = await BuildLogoAsync(partner.LogoMediaId, cancellationToken);

        var translations = partner.Translations
            .Select(t => new PartnerTranslationResponse(t.LanguageCode.Value, t.Name, t.Description))
            .ToList();

        var response = new PartnerDetailResponse(
            partner.Id, partner.LogoMediaId, logo, partner.WebsiteUrl, partner.SortOrder, partner.IsActive, partner.RowVersion, translations,
            partner.CreatedAtUtc);

        return Result.Success(response);
    }

    private async Task<PartnerLogoResponse?> BuildLogoAsync(Guid logoMediaId, CancellationToken cancellationToken)
    {
        var mediaAsset = await mediaAssetRepository.GetByIdAsync(logoMediaId, cancellationToken);
        if (mediaAsset is null)
        {
            return null;
        }

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

        return new PartnerLogoResponse(small, medium, large, originalUrl);
    }
}
