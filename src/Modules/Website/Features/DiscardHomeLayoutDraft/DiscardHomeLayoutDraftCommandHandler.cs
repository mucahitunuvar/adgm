using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DiscardHomeLayoutDraft;

public sealed class DiscardHomeLayoutDraftCommandHandler(
    IPageLayoutRepository pageLayoutRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DiscardHomeLayoutDraftCommand, Result>
{
    public async Task<Result> Handle(DiscardHomeLayoutDraftCommand request, CancellationToken cancellationToken)
    {
        var layout = await pageLayoutRepository.GetHomeAsync(cancellationToken)
            ?? throw new InvalidOperationException("The home page layout is missing its seeded row.");

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
