using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UnpublishContentItem;

public sealed class UnpublishContentItemCommandHandler(
    IContentItemRepository contentItemRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UnpublishContentItemCommand, Result>
{
    public async Task<Result> Handle(UnpublishContentItemCommand request, CancellationToken cancellationToken)
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

        var publishedChildCount = await contentItemRepository.CountPublishedChildrenAsync(contentItem.Id, cancellationToken);
        var unpublishResult = contentItem.Unpublish(publishedChildCount, currentUserContext.UserId!.Value, DateTime.UtcNow);
        if (unpublishResult.IsFailure)
        {
            return unpublishResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
