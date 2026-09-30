using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.ContentPaths;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.PermanentlyDeleteContentItem;

public sealed class PermanentlyDeleteContentItemCommandHandler(
    IContentItemRepository contentItemRepository,
    ContentItemPermanentDeletionService permanentDeletionService,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<PermanentlyDeleteContentItemCommand, Result>
{
    public async Task<Result> Handle(PermanentlyDeleteContentItemCommand request, CancellationToken cancellationToken)
    {
        var contentItem = await contentItemRepository.GetByIdAsync(request.Id, cancellationToken);
        if (contentItem is null)
        {
            return Result.Failure(Error.NotFound("ContentItem.NotFound", $"Content item '{request.Id}' could not be found."));
        }

        if (contentItem.DeletedAtUtc is null)
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.NotInTrash", "Only a content item already in the trash can be permanently deleted."));
        }

        var deleteResult = await permanentDeletionService.DeleteAsync(
            contentItem, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
        if (deleteResult.IsFailure)
        {
            return deleteResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidatePublicContent(cacheService);

        return Result.Success();
    }
}
