using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes;

// §4.3 "referans verilen tüm varlıklar var olmalı (türleri de doğru olmalı)": every block type's
// GetReferences only reports WHAT it references (no repository access from inside a block type
// definition) - this is the one place those references are actually checked against the database,
// merged across every block in a layout so each referenced kind is queried at most once per save
// (§4.1 "referans doğrulaması"), not once per block. A layout holds at most PageLayout.MaxBlocks (30)
// blocks and this runs only on an admin's draft save/publish, never a public hot path, so a handful of
// single-id lookups per kind (the same IMediaAssetRepository.GetByIdAsync-per-id shape
// MediaImageReferenceGuard already uses) is deliberately preferred here over adding bulk
// GetByIdsAsync methods to five repository interfaces no other feature needs yet.
public sealed class PageLayoutReferenceValidator(
    IMediaAssetRepository mediaAssetRepository,
    IVideoRepository videoRepository,
    ISliderRepository sliderRepository,
    IImpactMetricRepository impactMetricRepository,
    IContentItemRepository contentItemRepository,
    IContentTypeRepository contentTypeRepository,
    IContentCategoryRepository contentCategoryRepository)
{
    public async Task<Result> ValidateAsync(IReadOnlyList<BlockReferenceSet> referenceSets, CancellationToken cancellationToken)
    {
        var merged = Merge(referenceSets);

        foreach (var mediaId in merged.ImageMediaIds.Distinct())
        {
            var check = await MediaImageReferenceGuard.CheckAsync(mediaId, "PageLayout", "Image", mediaAssetRepository, cancellationToken);
            if (check.IsFailure)
            {
                return check;
            }
        }

        foreach (var videoId in merged.VideoIds.Distinct())
        {
            if (await videoRepository.GetByIdAsync(videoId, cancellationToken) is null)
            {
                return Result.Failure(Error.NotFound("PageLayout.VideoNotFound", $"Video '{videoId}' could not be found."));
            }
        }

        foreach (var sliderId in merged.SliderIds.Distinct())
        {
            if (await sliderRepository.GetByIdAsync(sliderId, cancellationToken) is null)
            {
                return Result.Failure(Error.NotFound("PageLayout.SliderNotFound", $"Slider '{sliderId}' could not be found."));
            }
        }

        foreach (var metricId in merged.ImpactMetricIds.Distinct())
        {
            if (await impactMetricRepository.GetByIdAsync(metricId, cancellationToken) is null)
            {
                return Result.Failure(Error.NotFound("PageLayout.ImpactMetricNotFound", $"Impact metric '{metricId}' could not be found."));
            }
        }

        var distinctContentItemIds = merged.ContentItemIds.Distinct().ToList();
        if (distinctContentItemIds.Count > 0)
        {
            var items = await contentItemRepository.GetByIdsAsync(distinctContentItemIds, cancellationToken);
            if (items.Count != distinctContentItemIds.Count)
            {
                return Result.Failure(Error.NotFound(
                    "PageLayout.ContentItemNotFound", "One or more referenced content items could not be found."));
            }
        }

        IReadOnlyList<ContentType>? allContentTypes = null;
        if (merged.ContentTypeListingIds.Count > 0 || merged.ContentTypeReferences.Count > 0 || merged.EventContentTypeKeys.Count > 0)
        {
            allContentTypes = await contentTypeRepository.GetAllAsync(cancellationToken);
        }

        foreach (var contentTypeId in merged.ContentTypeListingIds.Distinct())
        {
            if (allContentTypes!.All(t => t.Id != contentTypeId))
            {
                return Result.Failure(Error.NotFound("PageLayout.ContentTypeNotFound", $"Content type '{contentTypeId}' could not be found."));
            }
        }

        foreach (var eventKey in merged.EventContentTypeKeys.Distinct(StringComparer.Ordinal))
        {
            var contentTypeResult = ResolveContentTypeByKey(eventKey, allContentTypes!);
            if (contentTypeResult.IsFailure)
            {
                return Result.Failure(contentTypeResult.Error);
            }

            if (!contentTypeResult.Value.SupportsEvent)
            {
                return Result.Failure(Error.Validation(
                    "PageLayout.ContentTypeDoesNotSupportEvent", $"Content type '{eventKey}' does not support events."));
            }
        }

        foreach (var reference in merged.ContentTypeReferences.Distinct())
        {
            var contentTypeResult = ResolveContentTypeByKey(reference.ContentTypeKey, allContentTypes!);
            if (contentTypeResult.IsFailure)
            {
                return Result.Failure(contentTypeResult.Error);
            }

            if (reference.CategoryId is not { } categoryId)
            {
                continue;
            }

            var category = await contentCategoryRepository.GetByIdAsync(categoryId, cancellationToken);
            if (category is null)
            {
                return Result.Failure(Error.NotFound("PageLayout.ContentCategoryNotFound", $"Content category '{categoryId}' could not be found."));
            }

            if (category.ContentTypeId != contentTypeResult.Value.Id)
            {
                return Result.Failure(Error.Validation(
                    "PageLayout.CategoryDoesNotBelongToContentType",
                    $"Category '{categoryId}' does not belong to content type '{reference.ContentTypeKey}'."));
            }
        }

        return Result.Success();
    }

    private static Result<ContentType> ResolveContentTypeByKey(string key, IReadOnlyList<ContentType> allContentTypes)
    {
        var keyResult = ContentTypeKey.Create(key);
        if (keyResult.IsFailure)
        {
            return Result.Failure<ContentType>(keyResult.Error);
        }

        var contentType = allContentTypes.FirstOrDefault(t => t.Key == keyResult.Value);

        return contentType is null
            ? Result.Failure<ContentType>(Error.NotFound("PageLayout.ContentTypeNotFound", $"Content type '{key}' could not be found."))
            : Result.Success(contentType);
    }

    private static BlockReferenceSet Merge(IReadOnlyList<BlockReferenceSet> sets) =>
        new(
            sets.SelectMany(s => s.ImageMediaIds).ToList(),
            sets.SelectMany(s => s.VideoIds).ToList(),
            sets.SelectMany(s => s.SliderIds).ToList(),
            sets.SelectMany(s => s.ImpactMetricIds).ToList(),
            sets.SelectMany(s => s.ContentItemIds).ToList(),
            sets.SelectMany(s => s.ContentTypeListingIds).ToList(),
            sets.SelectMany(s => s.ContentTypeReferences).ToList(),
            sets.SelectMany(s => s.EventContentTypeKeys).ToList());
}
