using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemCategories;

public sealed class SetContentItemCategoriesCommandHandler(
    IContentItemRepository contentItemRepository,
    IContentTypeRepository contentTypeRepository,
    IContentCategoryRepository contentCategoryRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<SetContentItemCategoriesCommand, Result>
{
    public async Task<Result> Handle(SetContentItemCategoriesCommand request, CancellationToken cancellationToken)
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

        var contentType = await contentTypeRepository.GetByIdAsync(contentItem.ContentTypeId, cancellationToken);
        if (contentType is null || !contentType.SupportsCategories)
        {
            return Result.Failure(Error.Validation(
                "ContentItem.ContentTypeDoesNotSupportCategories", "This content item's content type does not support categories."));
        }

        var distinctIds = request.CategoryIds.Distinct().ToList();
        foreach (var categoryId in distinctIds)
        {
            var category = await contentCategoryRepository.GetByIdAsync(categoryId, cancellationToken);
            if (category is null || category.ContentTypeId != contentItem.ContentTypeId)
            {
                return Result.Failure(Error.NotFound(
                    "ContentCategory.NotFound", $"Category '{categoryId}' could not be found for this content item's content type."));
            }
        }

        var setResult = contentItem.SetCategories(distinctIds, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (setResult.IsFailure)
        {
            return setResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
