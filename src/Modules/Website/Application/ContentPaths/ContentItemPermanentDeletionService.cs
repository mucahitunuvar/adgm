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
    IMenuRepository menuRepository,
    IPageLayoutRepository pageLayoutRepository,
    IEventScheduleRepository eventScheduleRepository,
    IEventRegistrationUsageChecker eventRegistrationUsageChecker,
    IContentItemRevisionRepository contentItemRevisionRepository,
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

        // ADR-024 §11.1/§11.2 (Faz 4 Görev 1): a content item whose event has registrations cannot be
        // permanently deleted - the usage checker is a no-op until Görev 3 adds EventRegistration, so
        // this never blocks anything yet, but the wiring is in place for when it does.
        var hasRegistrations = await eventRegistrationUsageChecker.HasRegistrationsAsync(contentItem.Id, cancellationToken);
        if (hasRegistrations)
        {
            return Result.Failure(Error.Conflict(
                "Event.HasRegistrations", "Cannot permanently delete: this event still has registrations."));
        }

        var eventSchedule = await eventScheduleRepository.GetByContentItemIdAsync(contentItem.Id, cancellationToken);
        if (eventSchedule is not null)
        {
            eventScheduleRepository.Remove(eventSchedule);
            logger.LogInformation(
                "Deleting event schedule {EventScheduleId} for permanently deleted content item {ContentItemId}.",
                eventSchedule.Id, contentItem.Id);
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

        var linkingMenus = await menuRepository.GetByLinkedContentItemIdAsync(contentItem.Id, cancellationToken);
        foreach (var menu in linkingMenus)
        {
            menu.DeactivateItemsLinkingToContent(contentItem.Id, actingUserId, now);
            logger.LogInformation(
                "Cleared and deactivated menu item(s) in menu '{MenuLocation}' linking to permanently deleted content item {ContentItemId}.",
                menu.Location, contentItem.Id);
        }

        // Faz 2 Görev 4 master prompt §4.3: "İçerik kalıcı silindiğinde düzeni de aynı transaction'da
        // silinir" - a Content-target PageLayout only ever exists for this one ContentItem, so it
        // cannot be left behind as an orphan.
        var layout = await pageLayoutRepository.GetByContentItemIdAsync(contentItem.Id, cancellationToken);
        if (layout is not null)
        {
            pageLayoutRepository.Remove(layout);
            logger.LogInformation(
                "Deleting page layout {PageLayoutId} for permanently deleted content item {ContentItemId}.", layout.Id, contentItem.Id);
        }

        contentItemRepository.Remove(contentItem);

        // ADR-024 §4 (Faz 5 Görev 7): "Kalıcı silme revizyonları siler" - unlike the trash itself, where
        // "çöp kutusunda revizyonlar korunur" (MoveToTrash never touches revisions).
        await contentItemRevisionRepository.DeleteAllForContentItemAsync(contentItem.Id, cancellationToken);

        return Result.Success();
    }
}
