using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentItem;

public sealed class UpdateContentItemCommandHandler(
    IContentItemRepository contentItemRepository,
    IContentTypeRepository contentTypeRepository,
    IMediaAssetRepository mediaAssetRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateContentItemCommand, Result>
{
    public async Task<Result> Handle(UpdateContentItemCommand request, CancellationToken cancellationToken)
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

        var coverImageCheck = await MediaImageReferenceGuard.CheckAsync(request.CoverImageMediaId, "ContentItem", "CoverImage", mediaAssetRepository, cancellationToken);
        if (coverImageCheck.IsFailure)
        {
            return coverImageCheck;
        }

        var detailImageCheck = await MediaImageReferenceGuard.CheckAsync(request.DetailImageMediaId, "ContentItem", "DetailImage", mediaAssetRepository, cancellationToken);
        if (detailImageCheck.IsFailure)
        {
            return detailImageCheck;
        }

        var updateResult = contentItem.UpdateCore(
            request.SortOrder, request.IsFeatured, request.CoverImageMediaId, request.DetailImageMediaId,
            contentType.SupportsDetailImage, currentUserContext.UserId!.Value, DateTime.UtcNow);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
