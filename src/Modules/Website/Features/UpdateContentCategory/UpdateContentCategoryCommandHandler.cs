using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentCategory;

public sealed class UpdateContentCategoryCommandHandler(
    IContentCategoryRepository contentCategoryRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateContentCategoryCommand, Result>
{
    public async Task<Result> Handle(UpdateContentCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await contentCategoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null || category.ContentTypeId != request.TypeId)
        {
            return Result.Failure(Error.NotFound("ContentCategory.NotFound", $"Category '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(category.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "ContentCategory.ConcurrencyConflict", "The category was changed by someone else. Reload and try again."));
        }

        var updateResult = category.UpdateCore(request.SortOrder, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
