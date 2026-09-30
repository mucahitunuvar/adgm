using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeleteContentCategory;

public sealed class DeleteContentCategoryCommandHandler(
    IContentCategoryRepository contentCategoryRepository,
    IContentItemRepository contentItemRepository,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteContentCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteContentCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await contentCategoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null || category.ContentTypeId != request.TypeId)
        {
            return Result.Failure(Error.NotFound("ContentCategory.NotFound", $"Category '{request.Id}' could not be found."));
        }

        var childCount = await contentCategoryRepository.CountChildrenAsync(category.Id, cancellationToken);
        if (childCount > 0)
        {
            return Result.Failure(Error.Conflict(
                "ContentCategory.HasChildren", $"Cannot delete: {childCount} sub-category(ies) depend on this category."));
        }

        var assignedContentCount = await contentItemRepository.CountByCategoryIdAsync(category.Id, cancellationToken);
        if (assignedContentCount > 0)
        {
            return Result.Failure(Error.Conflict(
                "ContentCategory.InUse", $"Cannot delete: {assignedContentCount} content item(s) are assigned to this category."));
        }

        contentCategoryRepository.Remove(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
