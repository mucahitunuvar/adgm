using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace GenclikMerkezi.Modules.Website.Application.ContentPaths;

// ADR-024 §4.5 (Faz 1b Görev 6): the permanent-deletion cascade, shared by
// PermanentlyDeleteContentItemCommandHandler (an admin's manual action) and
// PermanentlyDeleteExpiredTrashJob (Hangfire's 30-day sweep) so the cleanup logic exists in exactly
// one place. Gallery/attachment/video/category/tag relations need no explicit cleanup here: gallery
// and attachment child rows cascade-delete with the ContentItem row (owned-entity FK), and
// category/video/tag ids live as JSON columns on the same row - both disappear with the row itself.
// Only the two genuinely separate aggregates that can reference this item - Redirect and other
// ContentItems' RelatedContentItemIds - need their own cleanup. Callers still owe the eventual
// IUnitOfWork.SaveChangesAsync() that commits everything atomically in one transaction.
public sealed class ContentItemPermanentDeletionService(
    IContentItemRepository contentItemRepository,
    IRedirectRepository redirectRepository,
    ILogger<ContentItemPermanentDeletionService> logger)
{
    public async Task<Result> DeleteAsync(ContentItem contentItem, Guid actingUserId, DateTime now, CancellationToken cancellationToken)
    {
        var children = await contentItemRepository.GetChildrenAsync(contentItem.Id, cancellationToken);
        if (children.Count > 0)
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.HasChildren",
                $"Cannot permanently delete: {children.Count} child content item(s) still exist (in any status)."));
        }

        var redirects = await redirectRepository.GetByTargetContentItemIdAsync(contentItem.Id, cancellationToken);
        foreach (var redirect in redirects)
        {
            logger.LogInformation(
                "Deleting redirect {RedirectId} (from '{FromPath}') targeting permanently deleted content item {ContentItemId}.",
                redirect.Id, redirect.FromPath, contentItem.Id);
            redirectRepository.Remove(redirect);
        }

        var itemsRelatingToThisOne = await contentItemRepository.GetByRelatedContentItemIdAsync(contentItem.Id, cancellationToken);
        foreach (var relatingItem in itemsRelatingToThisOne)
        {
            var remaining = relatingItem.RelatedContentItemIds.Where(id => id != contentItem.Id).ToList();
            relatingItem.SetRelatedContent(remaining, actingUserId, now);
        }

        contentItemRepository.Remove(contentItem);

        return Result.Success();
    }
}
