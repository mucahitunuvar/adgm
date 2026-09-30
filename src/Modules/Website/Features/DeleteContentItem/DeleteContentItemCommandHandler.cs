using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeleteContentItem;

// ADR-024 §4.5 (Faz 1b Görev 6): moves a content item to the trash. Published content must be
// unpublished first (ContentItem.MoveToTrash's own check); a non-trashed child blocks the move here,
// at the Application layer, since it is a cross-aggregate check ContentItem cannot do itself.
public sealed class DeleteContentItemCommandHandler(
    IContentItemRepository contentItemRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteContentItemCommand, Result>
{
    public async Task<Result> Handle(DeleteContentItemCommand request, CancellationToken cancellationToken)
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

        var children = await contentItemRepository.GetChildrenAsync(contentItem.Id, cancellationToken);
        var nonTrashedChildren = children.Where(c => c.DeletedAtUtc is null).ToList();
        if (nonTrashedChildren.Count > 0)
        {
            var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
            var titles = nonTrashedChildren.Select(c => ResolveDisplayTitle(c, defaultLanguage));
            return Result.Failure(Error.Conflict(
                "ContentItem.HasNonTrashedChildren",
                $"Cannot move to trash: {nonTrashedChildren.Count} child content item(s) are not yet in the trash: {string.Join(", ", titles)}."));
        }

        var trashResult = contentItem.MoveToTrash(currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (trashResult.IsFailure)
        {
            return trashResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static string ResolveDisplayTitle(Domain.ContentItem item, Domain.SiteLanguage? defaultLanguage)
    {
        var translation = defaultLanguage is not null
            ? item.Translations.FirstOrDefault(t => t.LanguageCode == defaultLanguage.Code)
            : null;

        return (translation ?? item.Translations.FirstOrDefault())?.Title ?? item.Id.ToString();
    }
}
