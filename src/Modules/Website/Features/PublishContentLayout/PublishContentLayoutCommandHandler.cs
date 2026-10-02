using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.PublishContentLayout;

public sealed class PublishContentLayoutCommandHandler(
    IPageLayoutRepository pageLayoutRepository,
    ISiteLanguageRepository siteLanguageRepository,
    LayoutBlockInputProcessor layoutBlockInputProcessor,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<PublishContentLayoutCommand, Result>
{
    public async Task<Result> Handle(PublishContentLayoutCommand request, CancellationToken cancellationToken)
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

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No default site language is configured.");

        var revalidation = await layoutBlockInputProcessor.ValidateStoredBlocksAsync(
            layout.DraftBlocks, PageLayoutTargetKind.Content, defaultLanguage.Code, cancellationToken);
        if (revalidation.IsFailure)
        {
            return revalidation;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var publishResult = layout.Publish(currentUserContext.UserId!.Value, now);
        if (publishResult.IsFailure)
        {
            return publishResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
