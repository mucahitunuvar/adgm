using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemAttachments;

public sealed class SetContentItemAttachmentsCommandHandler(
    IContentItemRepository contentItemRepository,
    IContentTypeRepository contentTypeRepository,
    IMediaAssetRepository mediaAssetRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<SetContentItemAttachmentsCommand, Result>
{
    public async Task<Result> Handle(SetContentItemAttachmentsCommand request, CancellationToken cancellationToken)
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
        if (contentType is null || !contentType.SupportsAttachments)
        {
            return Result.Failure(Error.Validation(
                "ContentItem.ContentTypeDoesNotSupportAttachments", "This content item's content type does not support attachments."));
        }

        var items = new List<ContentItemAttachment>();
        foreach (var itemInput in request.Items)
        {
            var documentGuardResult = await MediaDocumentReferenceGuard.CheckAsync(
                itemInput.MediaAssetId, "ContentItem", "Attachment", mediaAssetRepository, cancellationToken);
            if (documentGuardResult.IsFailure)
            {
                return documentGuardResult;
            }

            var translations = new List<ContentItemAttachmentTranslation>();
            foreach (var translationInput in itemInput.Translations)
            {
                var languageCodeResult = LanguageCode.Create(translationInput.LanguageCode);
                if (languageCodeResult.IsFailure)
                {
                    return Result.Failure(languageCodeResult.Error);
                }

                var translationResult = ContentItemAttachmentTranslation.Create(languageCodeResult.Value, translationInput.DisplayNameOverride);
                if (translationResult.IsFailure)
                {
                    return Result.Failure(translationResult.Error);
                }

                translations.Add(translationResult.Value);
            }

            items.Add(ContentItemAttachment.Create(itemInput.MediaAssetId, itemInput.SortOrder, translations));
        }

        var setResult = contentItem.SetAttachments(items, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (setResult.IsFailure)
        {
            return setResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
