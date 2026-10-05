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
                    t.Seo.OgImageMediaId, t.Seo.CanonicalUrl, t.Seo.NoIndex),
                t.TagIds))
            .ToList();

        // OwnsMany child collections carry no guaranteed read-back order (unlike VideoIds' JSON
        // array) - sorted here by the same SortOrder the whole-list PUT stored, so a round trip
        // reflects the order the editor set (ADR-024 Faz 1b "Liste, istekteki sırayla saklanır").
        var galleryItems = contentItem.GalleryItems
            .OrderBy(g => g.SortOrder)
            .Select(g => new ContentItemGalleryItemResponse(
                g.MediaAssetId, g.SortOrder,
                g.Translations.Select(t => new ContentItemGalleryItemTranslationResponse(t.LanguageCode.Value, t.AltTextOverride, t.CaptionOverride)).ToList()))
            .ToList();

        var attachments = contentItem.Attachments
            .OrderBy(a => a.SortOrder)
            .Select(a => new ContentItemAttachmentResponse(
                a.MediaAssetId, a.SortOrder,
                a.Translations.Select(t => new ContentItemAttachmentTranslationResponse(t.LanguageCode.Value, t.DisplayNameOverride)).ToList()))
            .ToList();

        var response = new ContentItemDetailResponse(
            contentItem.Id, contentItem.ContentTypeId, contentItem.ParentId, contentItem.Status.ToString(),
            contentItem.PublishAtUtc, contentItem.UnpublishAtUtc, contentItem.SortOrder, contentItem.IsFeatured,
            contentItem.CoverImageMediaId, coverImageUrl, contentItem.DetailImageMediaId, detailImageUrl,
            contentItem.FormDefinitionId, contentItem.IsVisible(DateTime.UtcNow), contentItem.RowVersion, contentItem.CategoryIds, galleryItems,
            contentItem.VideoIds, attachments, contentItem.RelatedContentItemIds, translations);

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
