using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DiscardContentLayoutDraft;

public sealed class DiscardContentLayoutDraftCommandHandler(
    IPageLayoutRepository pageLayoutRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DiscardContentLayoutDraftCommand, Result>
{
    public async Task<Result> Handle(DiscardContentLayoutDraftCommand request, CancellationToken cancellationToken)
    {
        var layout = await pageLayoutRepository.GetByContentItemIdAsync(request.ContentItemId, cancellationToken);
        if (layout is null)
        {
            return Result.Failure(Error.NotFound(
                "PageLayout.NotFound", $"No page layout exists yet for content item '{request.ContentItemId}'."));
        }

        if (!request.RowVersion.SequenceEqual(layout.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "PageLayout.ConcurrencyConflict", "The page layout was changed by someone else. Reload and try again."));
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var discardResult = layout.DiscardDraft(currentUserContext.UserId!.Value, now);
        if (discardResult.IsFailure)
        {
            return discardResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
