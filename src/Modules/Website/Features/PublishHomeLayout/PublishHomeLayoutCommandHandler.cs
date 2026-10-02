using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.PublishHomeLayout;

// §4.3 "POST .../publish - bu anda referans doğrulaması tekrar yapılır": a draft block's reference
// (an ImpactMetric, or a Content/ContentType link target) is not protected by a usage checker the way
// media/video/slider are, so it could have been deleted since the draft was last saved - re-validated
// here before the draft becomes the published list.
public sealed class PublishHomeLayoutCommandHandler(
    IPageLayoutRepository pageLayoutRepository,
    ISiteLanguageRepository siteLanguageRepository,
    LayoutBlockInputProcessor layoutBlockInputProcessor,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<PublishHomeLayoutCommand, Result>
{
    public async Task<Result> Handle(PublishHomeLayoutCommand request, CancellationToken cancellationToken)
    {
        var layout = await pageLayoutRepository.GetHomeAsync(cancellationToken)
            ?? throw new InvalidOperationException("The home page layout is missing its seeded row.");

        if (!request.RowVersion.SequenceEqual(layout.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "PageLayout.ConcurrencyConflict", "The page layout was changed by someone else. Reload and try again."));
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No default site language is configured.");

        var revalidation = await layoutBlockInputProcessor.ValidateStoredBlocksAsync(
            layout.DraftBlocks, PageLayoutTargetKind.Home, defaultLanguage.Code, cancellationToken);
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
