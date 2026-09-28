using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentItemById;

public sealed class GetContentItemByIdQueryHandler(
    IContentItemRepository contentItemRepository, IMediaAssetRepository mediaAssetRepository, IFileStorageService fileStorageService)
    : IRequestHandler<GetContentItemByIdQuery, Result<ContentItemDetailResponse>>
{
    public async Task<Result<ContentItemDetailResponse>> Handle(GetContentItemByIdQuery request, CancellationToken cancellationToken)
    {
        var contentItem = await contentItemRepository.GetByIdAsync(request.Id, cancellationToken);
        if (contentItem is null)
        {
            return Result.Failure<ContentItemDetailResponse>(
                Error.NotFound("ContentItem.NotFound", $"Content item '{request.Id}' could not be found."));
        }

        var coverImageUrl = await ResolveMediaUrlAsync(contentItem.CoverImageMediaId, cancellationToken);
        var detailImageUrl = await ResolveMediaUrlAsync(contentItem.DetailImageMediaId, cancellationToken);

        var translations = contentItem.Translations
            .Select(t => new ContentItemTranslationResponse(
                t.LanguageCode.Value, t.Title, t.Slug, t.FullPath, t.Summary, t.Body,
                new ContentItemSeoResponse(
                    t.Seo.MetaTitle, t.Seo.MetaDescription, t.Seo.MetaKeywords, t.Seo.OgTitle, t.Seo.OgDescription,
                    t.Seo.OgImageMediaId, t.Seo.CanonicalUrl, t.Seo.NoIndex)))
            .ToList();

        var response = new ContentItemDetailResponse(
            contentItem.Id, contentItem.ContentTypeId, contentItem.ParentId, contentItem.Status.ToString(),
            contentItem.PublishAtUtc, contentItem.UnpublishAtUtc, contentItem.SortOrder, contentItem.IsFeatured,
            contentItem.CoverImageMediaId, coverImageUrl, contentItem.DetailImageMediaId, detailImageUrl,
            contentItem.IsVisible(DateTime.UtcNow), contentItem.RowVersion, translations);

        return Result.Success(response);
    }

    private async Task<string?> ResolveMediaUrlAsync(Guid? mediaAssetId, CancellationToken cancellationToken)
    {
        if (mediaAssetId is null)
        {
            return null;
        }

        var mediaAsset = await mediaAssetRepository.GetByIdAsync(mediaAssetId.Value, cancellationToken);
        return mediaAsset is null ? null : await fileStorageService.GetUrlAsync(mediaAsset.Original.FileKey, cancellationToken);
    }
}
