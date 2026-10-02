using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPartners;

public sealed class GetPartnersQueryHandler(
    IPartnerRepository partnerRepository,
    ISiteLanguageRepository siteLanguageRepository,
    IMediaAssetRepository mediaAssetRepository,
    IFileStorageService fileStorageService)
    : IRequestHandler<GetPartnersQuery, Result<PagedResult<PartnerSummaryResponse>>>
{
    public async Task<Result<PagedResult<PartnerSummaryResponse>>> Handle(GetPartnersQuery request, CancellationToken cancellationToken)
    {
        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure<PagedResult<PartnerSummaryResponse>>(
                Error.Failure("Partner.NoDefaultLanguage", "No default site language is configured."));
        }

        var paged = await partnerRepository.SearchAsync(request.IsActive, request.Search, defaultLanguage.Code, request, cancellationToken);

        var items = new List<PartnerSummaryResponse>();
        foreach (var partner in paged.Items)
        {
            items.Add(await ToSummaryAsync(partner, defaultLanguage.Code, cancellationToken));
        }

        return Result.Success(new PagedResult<PartnerSummaryResponse>(items, paged.TotalCount, paged.Page, paged.PageSize));
    }

    private async Task<PartnerSummaryResponse> ToSummaryAsync(Partner partner, LanguageCode defaultLanguageCode, CancellationToken cancellationToken)
    {
        var name = partner.Translations.FirstOrDefault(t => t.LanguageCode == defaultLanguageCode)?.Name ?? string.Empty;
        var logo = await BuildLogoAsync(partner.LogoMediaId, cancellationToken);

        return new PartnerSummaryResponse(partner.Id, logo, partner.WebsiteUrl, name, partner.SortOrder, partner.IsActive, partner.RowVersion);
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
