using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemRelatedContent;

public sealed class SetContentItemRelatedContentCommandHandler(
    IContentItemRepository contentItemRepository,
    IContentTypeRepository contentTypeRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<SetContentItemRelatedContentCommand, Result>
{
    public async Task<Result> Handle(SetContentItemRelatedContentCommand request, CancellationToken cancellationToken)
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
        if (contentType is null || !contentType.SupportsRelatedContent)
        {
            return Result.Failure(Error.Validation(
                "ContentItem.ContentTypeDoesNotSupportRelatedContent", "This content item's content type does not support related content."));
        }

        // The target may be of any ContentType ("hedef herhangi bir türden olabilir") - only
        // existence is checked here, not the target's own feature flags.
        foreach (var relatedId in request.RelatedContentItemIds)
        {
            var related = await contentItemRepository.GetByIdAsync(relatedId, cancellationToken);
            if (related is null)
            {
                return Result.Failure(Error.NotFound("ContentItem.RelatedContentItemNotFound", $"Content item '{relatedId}' could not be found."));
            }
        }

        var setResult = contentItem.SetRelatedContent(
            request.RelatedContentItemIds, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (setResult.IsFailure)
        {
            return setResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
