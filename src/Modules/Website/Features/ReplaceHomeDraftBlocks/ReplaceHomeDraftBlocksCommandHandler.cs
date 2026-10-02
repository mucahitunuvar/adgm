using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.ReplaceHomeDraftBlocks;

// §4.3 "PUT .../layouts/home/draft - taslak blok listesinin tamamı". The Home PageLayout always
// exists (seeded by migration, like the three Menu rows), so unlike the Content variant there is no
// lazy-create branch here.
public sealed class ReplaceHomeDraftBlocksCommandHandler(
    IPageLayoutRepository pageLayoutRepository,
    ISiteLanguageRepository siteLanguageRepository,
    LayoutBlockInputProcessor layoutBlockInputProcessor,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ReplaceHomeDraftBlocksCommand, Result>
{
    public async Task<Result> Handle(ReplaceHomeDraftBlocksCommand request, CancellationToken cancellationToken)
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

        var blocksResult = await layoutBlockInputProcessor.ProcessAsync(
            request.Blocks, PageLayoutTargetKind.Home, defaultLanguage.Code, cancellationToken);
        if (blocksResult.IsFailure)
        {
            return Result.Failure(blocksResult.Error);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var replaceResult = layout.ReplaceDraftBlocks(blocksResult.Value, currentUserContext.UserId!.Value, now);
        if (replaceResult.IsFailure)
        {
            return replaceResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
