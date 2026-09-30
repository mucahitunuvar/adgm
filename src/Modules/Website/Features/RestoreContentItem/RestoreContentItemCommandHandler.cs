using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.RestoreContentItem;

// ADR-024 §4.5 (Faz 1b Görev 6): restores a trashed item to its status before deletion. Whether the
// parent is itself still in the trash is a cross-aggregate check the Application layer performs -
// ContentItem.Restore has no parent access of its own.
public sealed class RestoreContentItemCommandHandler(
    IContentItemRepository contentItemRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<RestoreContentItemCommand, Result>
{
    public async Task<Result> Handle(RestoreContentItemCommand request, CancellationToken cancellationToken)
    {
        var contentItem = await contentItemRepository.GetByIdAsync(request.Id, cancellationToken);
        if (contentItem is null)
        {
            return Result.Failure(Error.NotFound("ContentItem.NotFound", $"Content item '{request.Id}' could not be found."));
        }

        if (contentItem.ParentId is not null)
        {
            var parent = await contentItemRepository.GetByIdAsync(contentItem.ParentId.Value, cancellationToken);
            if (parent is not null && parent.DeletedAtUtc is not null)
            {
                return Result.Failure(Error.Conflict(
                    "ContentItem.ParentInTrash", "The parent content item is in the trash; restore it first."));
            }
        }

        var restoreResult = contentItem.Restore(currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (restoreResult.IsFailure)
        {
            return restoreResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
