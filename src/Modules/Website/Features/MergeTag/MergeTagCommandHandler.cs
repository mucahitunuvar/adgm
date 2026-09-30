using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.MergeTag;

// ADR-024 §4.1 (Faz 1b Görev 3): every content item referencing Id now references TargetId instead
// (deduplicated, in case a translation already carried both - ContentItem.SetTranslationTags already
// deduplicates); Id itself is then deleted.
public sealed class MergeTagCommandHandler(
    IContentTagRepository contentTagRepository,
    IContentItemRepository contentItemRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<MergeTagCommand, Result>
{
    public async Task<Result> Handle(MergeTagCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == request.TargetId)
        {
            return Result.Failure(Error.Validation("ContentTag.CannotMergeIntoItself", "A tag cannot be merged into itself."));
        }

        var source = await contentTagRepository.GetByIdAsync(request.Id, cancellationToken);
        if (source is null)
        {
            return Result.Failure(Error.NotFound("ContentTag.NotFound", $"Tag '{request.Id}' could not be found."));
        }

        var target = await contentTagRepository.GetByIdAsync(request.TargetId, cancellationToken);
        if (target is null)
        {
            return Result.Failure(Error.NotFound("ContentTag.NotFound", $"Tag '{request.TargetId}' could not be found."));
        }

        if (source.LanguageCode != target.LanguageCode)
        {
            return Result.Failure(Error.Validation(
                "ContentTag.MergeLanguageMismatch", "Both tags must belong to the same language to be merged."));
        }

        var affectedItems = await contentItemRepository.GetByTagIdAsync(source.Id, cancellationToken);
        var userId = currentUserContext.UserId!.Value;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var item in affectedItems)
        {
            foreach (var translation in item.Translations.Where(t => t.TagIds.Contains(source.Id)))
            {
                var mergedTagIds = translation.TagIds.Where(id => id != source.Id).Append(target.Id).Distinct().ToList();
                var setResult = item.SetTranslationTags(translation.LanguageCode, mergedTagIds, userId, now);
                if (setResult.IsFailure)
                {
                    return setResult;
                }
            }
        }

        contentTagRepository.Remove(source);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidatePublicContent(cacheService);

        return Result.Success();
    }
}
