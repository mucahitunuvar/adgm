using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.PublishContentItem;

public sealed class PublishContentItemCommandHandler(
    IContentItemRepository contentItemRepository,
    IContentTypeRepository contentTypeRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ISearchIndexUpdater searchIndexUpdater,
    IContentRevisionRecorder contentRevisionRecorder,
    ICurrentUserContext currentUserContext,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<PublishContentItemCommand, Result>
{
    public async Task<Result> Handle(PublishContentItemCommand request, CancellationToken cancellationToken)
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
        if (contentType is null)
        {
            return Result.Failure(Error.Failure("ContentItem.ContentTypeNotFound", "The content item's content type could not be found."));
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure(Error.Failure("ContentItem.NoDefaultLanguage", "No default site language is configured."));
        }

        var parentIsPublished = true;
        if (contentItem.ParentId is not null)
        {
            var parent = await contentItemRepository.GetByIdAsync(contentItem.ParentId.Value, cancellationToken);
            parentIsPublished = parent?.Status == ContentItemStatus.Published;
        }

        var userId = currentUserContext.UserId!.Value;
        var now = DateTime.UtcNow;

        var publishResult = contentItem.Publish(
            request.PublishAtUtc, request.UnpublishAtUtc, contentType.IsActive, parentIsPublished, defaultLanguage.Code, userId, now);
        if (publishResult.IsFailure)
        {
            return publishResult;
        }

        await searchIndexUpdater.ReindexWithDescendantsAsync(contentItem, cancellationToken);

        // ADR-024 §4 (Faz 5 Görev 7): "içerik yayınlandığı anda revizyon Kind = Published,
        // IsPublishedSnapshot = true olur" - always recorded, even if byte-identical to the previous
        // revision (ContentRevisionRecorder never hash-dedupes a Published snapshot).
        await contentRevisionRecorder.RecordAsync(contentItem, ContentItemRevisionKind.Published, [], userId, now, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
