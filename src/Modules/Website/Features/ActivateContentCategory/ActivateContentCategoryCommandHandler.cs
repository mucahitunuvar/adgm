using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.ActivateContentCategory;

public sealed class ActivateContentCategoryCommandHandler(
    IContentCategoryRepository contentCategoryRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ActivateContentCategoryCommand, Result>
{
    public async Task<Result> Handle(ActivateContentCategoryCommand request, CancellationToken cancellationToken)
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

        var activateResult = category.Activate(currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (activateResult.IsFailure)
        {
            return activateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
