using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.ReplaceContentDraftBlocks;

// §4.3 "PUT .../layouts/content/{contentItemId}/draft ... İçerik düzeni ilk kayıtta oluşur": unlike
// the Home variant, the PageLayout row may not exist yet - this handler creates it on the first save
// and skips the RowVersion check in that case (there is nothing yet to conflict against).
public sealed class ReplaceContentDraftBlocksCommandHandler(
    IPageLayoutRepository pageLayoutRepository,
    IContentItemRepository contentItemRepository,
    IContentTypeRepository contentTypeRepository,
    ISiteLanguageRepository siteLanguageRepository,
    LayoutBlockInputProcessor layoutBlockInputProcessor,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ReplaceContentDraftBlocksCommand, Result>
{
    public async Task<Result> Handle(ReplaceContentDraftBlocksCommand request, CancellationToken cancellationToken)
    {
        var contentItem = await contentItemRepository.GetByIdAsync(request.ContentItemId, cancellationToken);
        if (contentItem is null || contentItem.DeletedAtUtc is not null)
        {
            return Result.Failure(Error.NotFound("ContentItem.NotFound", $"Content item '{request.ContentItemId}' could not be found."));
        }

        var contentType = await contentTypeRepository.GetByIdAsync(contentItem.ContentTypeId, cancellationToken)
            ?? throw new InvalidOperationException("A content item references a content type that no longer exists.");

        if (!contentType.SupportsBlockLayout)
        {
            return Result.Failure(Error.Validation(
                "PageLayout.ContentTypeDoesNotSupportBlockLayout",
                $"Content type '{contentType.Key}' does not support page layouts."));
        }

        var layout = await pageLayoutRepository.GetByContentItemIdAsync(request.ContentItemId, cancellationToken);

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No default site language is configured.");

        var blocksResult = await layoutBlockInputProcessor.ProcessAsync(
            request.Blocks, PageLayoutTargetKind.Content, defaultLanguage.Code, cancellationToken);
        if (blocksResult.IsFailure)
        {
            return Result.Failure(blocksResult.Error);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var userId = currentUserContext.UserId!.Value;

        if (layout is null)
        {
            var createResult = PageLayout.CreateForContent(request.ContentItemId, userId, now);
            if (createResult.IsFailure)
            {
                return Result.Failure(createResult.Error);
            }

            layout = createResult.Value;
            pageLayoutRepository.Add(layout);
        }
        else if (!request.RowVersion.SequenceEqual(layout.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "PageLayout.ConcurrencyConflict", "The page layout was changed by someone else. Reload and try again."));
        }

        var replaceResult = layout.ReplaceDraftBlocks(blocksResult.Value, userId, now);
        if (replaceResult.IsFailure)
        {
            return replaceResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
