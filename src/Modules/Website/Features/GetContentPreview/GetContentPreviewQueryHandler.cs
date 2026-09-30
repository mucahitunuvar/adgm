using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.ContentPaths;
using GenclikMerkezi.Modules.Website.Application.Media;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentPreview;

// ADR-024 §4.5 (Faz 1b Görev 6): unlike the public detail endpoint Görev 7 adds, this deliberately
// skips every visibility check (own status/schedule and every ancestor's) - a preview link exists
// specifically to show a Draft/Unpublished page before it is visible to anyone else. The only gate is
// the trash: a permanently-recoverable trashed item can still be previewed via Görev 6's restore flow,
// but showing its content through a stale preview link the moment it is trashed would defeat the
// point of moving it out of sight, so it 404s exactly like a genuinely missing content item.
public sealed class GetContentPreviewQueryHandler(
    IContentPreviewLinkGenerator previewLinkGenerator,
    IContentItemRepository contentItemRepository,
    IContentTypeRepository contentTypeRepository,
    ISiteLanguageRepository siteLanguageRepository,
    IMediaAssetRepository mediaAssetRepository,
    IFileStorageService fileStorageService,
    IVideoRepository videoRepository,
    IContentCategoryRepository contentCategoryRepository,
    IContentTagRepository contentTagRepository,
    RelatedContentResolutionService relatedContentResolutionService,
    TimeProvider timeProvider)
    : IRequestHandler<GetContentPreviewQuery, Result<ContentPreviewResponse>>
{
    private static readonly Error InvalidTokenError = Error.NotFound("ContentPreview.NotFound", "This preview link is invalid or has expired.");

    public async Task<Result<ContentPreviewResponse>> Handle(GetContentPreviewQuery request, CancellationToken cancellationToken)
    {
        var tokenResult = previewLinkGenerator.ValidateToken(request.Token);
        if (tokenResult.IsFailure)
        {
            return Result.Failure<ContentPreviewResponse>(InvalidTokenError);
        }

        var contentItem = await contentItemRepository.GetByIdAsync(tokenResult.Value.ContentItemId, cancellationToken);
        if (contentItem is null || contentItem.DeletedAtUtc is not null)
        {
            return Result.Failure<ContentPreviewResponse>(InvalidTokenError);
        }

        var contentType = await contentTypeRepository.GetByIdAsync(contentItem.ContentTypeId, cancellationToken);
        if (contentType is null)
        {
            return Result.Failure<ContentPreviewResponse>(InvalidTokenError);
        }

        var languageCode = await ResolveLanguageCodeAsync(tokenResult.Value.LanguageCode, cancellationToken);
        var translation = contentItem.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (translation is null)
        {
            return Result.Failure<ContentPreviewResponse>(InvalidTokenError);
        }

        var coverImage = await BuildImageAsync(contentItem.CoverImageMediaId, cancellationToken);
        var detailImage = contentType.SupportsDetailImage ? await BuildImageAsync(contentItem.DetailImageMediaId, cancellationToken) : null;

        var gallery = contentType.SupportsGallery
            ? await BuildGalleryAsync(contentItem, languageCode, cancellationToken)
            : [];

        var videos = contentType.SupportsVideos
            ? await BuildVideosAsync(contentItem.VideoIds, languageCode, cancellationToken)
            : [];

        var attachments = contentType.SupportsAttachments
            ? await BuildAttachmentsAsync(contentItem, languageCode, cancellationToken)
            : [];

        var categories = contentType.SupportsCategories
            ? await BuildCategoryNamesAsync(contentItem.CategoryIds, languageCode, cancellationToken)
            : [];

        var tags = contentType.SupportsTags
            ? await BuildTagNamesAsync(translation.TagIds, cancellationToken)
            : [];

        var related = contentType.SupportsRelatedContent
            ? await BuildRelatedAsync(contentItem, languageCode, cancellationToken)
            : [];

        return Result.Success(new ContentPreviewResponse(
            contentItem.Id, contentType.Key.Value, contentType.DetailTemplate, translation.Title, translation.Summary, translation.Body,
            coverImage, detailImage, gallery, videos, attachments, categories, tags, related));
    }

    private async Task<LanguageCode> ResolveLanguageCodeAsync(string? tokenLanguageCode, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(tokenLanguageCode))
        {
            var parsed = LanguageCode.Create(tokenLanguageCode);
            if (parsed.IsSuccess)
            {
                return parsed.Value;
            }
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        return defaultLanguage!.Code;
    }

    private async Task<ContentPreviewImageResponse?> BuildImageAsync(Guid? mediaAssetId, CancellationToken cancellationToken)
    {
        if (mediaAssetId is null)
        {
            return null;
        }

        var mediaAsset = await mediaAssetRepository.GetByIdAsync(mediaAssetId.Value, cancellationToken);
        if (mediaAsset is null)
        {
            return null;
        }

        return await ToImageResponseAsync(mediaAsset, cancellationToken);
    }

    private async Task<ContentPreviewImageResponse> ToImageResponseAsync(MediaAsset mediaAsset, CancellationToken cancellationToken)
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

        return new ContentPreviewImageResponse(small, medium, large, originalUrl);
    }

    private async Task<IReadOnlyList<ContentPreviewGalleryItemResponse>> BuildGalleryAsync(
        ContentItem contentItem, LanguageCode languageCode, CancellationToken cancellationToken)
    {
        var items = new List<ContentPreviewGalleryItemResponse>();
        foreach (var galleryItem in contentItem.GalleryItems.OrderBy(g => g.SortOrder))
        {
            var mediaAsset = await mediaAssetRepository.GetByIdAsync(galleryItem.MediaAssetId, cancellationToken);
            if (mediaAsset is null)
            {
                continue;
            }

            var overrideTranslation = galleryItem.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
            var mediaTranslation = mediaAsset.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
            var altText = GalleryItemDisplayResolver.ResolveAltText(overrideTranslation?.AltTextOverride, mediaTranslation?.AltText);
            var caption = GalleryItemDisplayResolver.ResolveCaption(overrideTranslation?.CaptionOverride, mediaTranslation?.Caption);

            items.Add(new ContentPreviewGalleryItemResponse(
                mediaAsset.Id, await ToImageResponseAsync(mediaAsset, cancellationToken), altText, caption));
        }

        return items;
    }

    private async Task<IReadOnlyList<ContentPreviewVideoResponse>> BuildVideosAsync(
        IReadOnlyList<Guid> videoIds, LanguageCode languageCode, CancellationToken cancellationToken)
    {
        var items = new List<ContentPreviewVideoResponse>();
        foreach (var videoId in videoIds)
        {
            var video = await videoRepository.GetByIdAsync(videoId, cancellationToken);
            if (video is null || !video.IsActive)
            {
                continue;
            }

            var translation = video.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
            if (translation is null)
            {
                continue;
            }

            items.Add(new ContentPreviewVideoResponse(
                video.Id, video.YouTubeVideoId.Value, video.YouTubeVideoId.BuildEmbedUrl(), video.YouTubeVideoId.BuildThumbnailUrl(),
                translation.Title, translation.Description));
        }

        return items;
    }

    private async Task<IReadOnlyList<ContentPreviewAttachmentResponse>> BuildAttachmentsAsync(
        ContentItem contentItem, LanguageCode languageCode, CancellationToken cancellationToken)
    {
        var items = new List<ContentPreviewAttachmentResponse>();
        foreach (var attachment in contentItem.Attachments.OrderBy(a => a.SortOrder))
        {
            var mediaAsset = await mediaAssetRepository.GetByIdAsync(attachment.MediaAssetId, cancellationToken);
            if (mediaAsset is null)
            {
                continue;
            }

            var overrideTranslation = attachment.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
            var name = AttachmentDisplayNameResolver.Resolve(overrideTranslation?.DisplayNameOverride, mediaAsset.Original.OriginalFileName);
            var url = await fileStorageService.GetUrlAsync(mediaAsset.Original.FileKey, cancellationToken);
            var extension = Path.GetExtension(mediaAsset.Original.OriginalFileName).TrimStart('.');

            items.Add(new ContentPreviewAttachmentResponse(url, name, extension, mediaAsset.Original.SizeInBytes));
        }

        return items;
    }

    private async Task<IReadOnlyList<string>> BuildCategoryNamesAsync(
        IReadOnlyList<Guid> categoryIds, LanguageCode languageCode, CancellationToken cancellationToken)
    {
        var names = new List<string>();
        foreach (var categoryId in categoryIds)
        {
            var category = await contentCategoryRepository.GetByIdAsync(categoryId, cancellationToken);
            var translation = category?.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
            if (translation is not null)
            {
                names.Add(translation.Name);
            }
        }

        return names;
    }

    private async Task<IReadOnlyList<string>> BuildTagNamesAsync(IReadOnlyList<Guid> tagIds, CancellationToken cancellationToken)
    {
        var names = new List<string>();
        foreach (var tagId in tagIds)
        {
            var tag = await contentTagRepository.GetByIdAsync(tagId, cancellationToken);
            if (tag is not null)
            {
                names.Add(tag.Name);
            }
        }

        return names;
    }

    private async Task<IReadOnlyList<ContentPreviewRelatedItemResponse>> BuildRelatedAsync(
        ContentItem contentItem, LanguageCode languageCode, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var candidates = await relatedContentResolutionService.ResolveAsync(
            contentItem.Id, contentItem.ContentTypeId, contentItem.RelatedContentItemIds, contentItem.CategoryIds, languageCode, now,
            cancellationToken);

        var items = new List<ContentPreviewRelatedItemResponse>();
        foreach (var candidate in candidates)
        {
            var image = await BuildImageAsync(candidate.CoverImageMediaId, cancellationToken);
            items.Add(new ContentPreviewRelatedItemResponse(candidate.Id, candidate.Title, candidate.Summary, image?.Original));
        }

        return items;
    }
}
