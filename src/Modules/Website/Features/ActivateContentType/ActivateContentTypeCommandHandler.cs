using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.ActivateContentType;

public sealed class ActivateContentTypeCommandHandler(
    IContentTypeRepository contentTypeRepository,
    ISearchIndexUpdater searchIndexUpdater,
    ICurrentUserContext currentUserContext,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ActivateContentTypeCommand, Result>
{
    public async Task<Result> Handle(ActivateContentTypeCommand request, CancellationToken cancellationToken)
    {
        var contentType = await contentTypeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (contentType is null)
        {
            return Result.Failure(Error.NotFound("ContentType.NotFound", $"Content type '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(contentType.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "ContentType.ConcurrencyConflict", "The content type was changed by someone else. Reload and try again."));
        }

        var activateResult = contentType.Activate(currentUserContext.UserId!.Value, DateTime.UtcNow);
        if (activateResult.IsFailure)
        {
            return activateResult;
        }

        await searchIndexUpdater.ReindexContentTypeAsync(contentType.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
