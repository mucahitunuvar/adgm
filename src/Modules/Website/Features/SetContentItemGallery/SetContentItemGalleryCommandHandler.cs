using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemGallery;

public sealed class SetContentItemGalleryCommandHandler(
    IContentItemRepository contentItemRepository,
    IContentTypeRepository contentTypeRepository,
    IMediaAssetRepository mediaAssetRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<SetContentItemGalleryCommand, Result>
{
    public async Task<Result> Handle(SetContentItemGalleryCommand request, CancellationToken cancellationToken)
    {
        var contentItem = await contentItemRepository.GetByIdAsync(request.Id, cancellationToken);
        if (contentItem is null)
        {
            return Result.Failure(Error.NotFound("ContentItem.NotFound", $"Content item '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(contentItem.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.ConcurrencyConflict", "The content item was changed by someone else. Reload and try again."));
        }

        var trashCheck = ContentItemTrashGuard.EnsureEditable(contentItem);
        if (trashCheck.IsFailure)
        {
            return trashCheck;
        }

        var contentType = await contentTypeRepository.GetByIdAsync(contentItem.ContentTypeId, cancellationToken);
        if (contentType is null || !contentType.SupportsGallery)
        {
            return Result.Failure(Error.Validation(
                "ContentItem.ContentTypeDoesNotSupportGallery", "This content item's content type does not support a gallery."));
        }

        var items = new List<ContentItemGalleryItem>();
        foreach (var itemInput in request.Items)
        {
            var imageGuardResult = await MediaImageReferenceGuard.CheckAsync(
                itemInput.MediaAssetId, "ContentItem", "GalleryItem", mediaAssetRepository, cancellationToken);
            if (imageGuardResult.IsFailure)
            {
                return imageGuardResult;
            }

            var translations = new List<ContentItemGalleryItemTranslation>();
            foreach (var translationInput in itemInput.Translations)
            {
                var languageCodeResult = LanguageCode.Create(translationInput.LanguageCode);
                if (languageCodeResult.IsFailure)
                {
                    return Result.Failure(languageCodeResult.Error);
                }

                var translationResult = ContentItemGalleryItemTranslation.Create(
                    languageCodeResult.Value, translationInput.AltTextOverride, translationInput.CaptionOverride);
                if (translationResult.IsFailure)
                {
                    return Result.Failure(translationResult.Error);
                }

                translations.Add(translationResult.Value);
            }

            items.Add(ContentItemGalleryItem.Create(itemInput.MediaAssetId, itemInput.SortOrder, translations));
        }

        var setResult = contentItem.SetGallery(items, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (setResult.IsFailure)
        {
            return setResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidatePublicContent(cacheService);

        return Result.Success();
    }
}
