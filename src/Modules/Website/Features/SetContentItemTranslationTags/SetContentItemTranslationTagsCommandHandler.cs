using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemTranslationTags;

// ADR-024 §4.1 (Faz 1b Görev 3): tag names are found-or-created by normalized slug ("Gençlik" and
// "gençlik" are the same tag) - the editor writes free text, this handler is the only place that
// resolves it to ContentTag ids.
public sealed class SetContentItemTranslationTagsCommandHandler(
    IContentItemRepository contentItemRepository,
    IContentTypeRepository contentTypeRepository,
    IContentTagRepository contentTagRepository,
    IContentRevisionRecorder contentRevisionRecorder,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<SetContentItemTranslationTagsCommand, Result>
{
    public async Task<Result> Handle(SetContentItemTranslationTagsCommand request, CancellationToken cancellationToken)
    {
        var contentItem = await contentItemRepository.GetByIdAsync(request.Id, cancellationToken);
        if (contentItem is null)
        {
            return Result.Failure(Error.NotFound("ContentItem.NotFound", $"Content item '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(contentItem.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.ConcurrencyConflict", "The content item was changed by someone else. Reload and try again."));
        }

        var trashCheck = ContentItemTrashGuard.EnsureEditable(contentItem);
        if (trashCheck.IsFailure)
        {
            return trashCheck;
        }

        var contentType = await contentTypeRepository.GetByIdAsync(contentItem.ContentTypeId, cancellationToken);
        if (contentType is null || !contentType.SupportsTags)
        {
            return Result.Failure(Error.Validation(
                "ContentItem.ContentTypeDoesNotSupportTags", "This content item's content type does not support tags."));
        }

        var languageCodeResult = LanguageCode.Create(request.LanguageCode);
        if (languageCodeResult.IsFailure)
        {
            return languageCodeResult;
        }

        var languageCode = languageCodeResult.Value;
        var userId = currentUserContext.UserId!.Value;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        // "boş/tekrarlı adlar temizlenir" (ADR-024 §4.1) - de-duplicated by normalized slug, not raw
        // text, so "Gençlik" and "gençlik" collapse to one find-or-create instead of two.
        var resolvedTagIds = new List<Guid>();
        var seenSlugs = new HashSet<string>();

        foreach (var rawName in request.TagNames)
        {
            var trimmed = (rawName ?? string.Empty).Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            var slugResult = Slug.Create(trimmed);
            if (slugResult.IsFailure)
            {
                return Result.Failure(slugResult.Error);
            }

            if (!seenSlugs.Add(slugResult.Value.Value))
            {
                continue;
            }

            var existingTag = await contentTagRepository.GetBySlugAsync(languageCode, slugResult.Value.Value, cancellationToken);
            if (existingTag is not null)
            {
                resolvedTagIds.Add(existingTag.Id);
                continue;
            }

            var createResult = ContentTag.Create(languageCode, trimmed, userId, now);
            if (createResult.IsFailure)
            {
                return Result.Failure(createResult.Error);
            }

            contentTagRepository.Add(createResult.Value);
            resolvedTagIds.Add(createResult.Value.Id);
        }

        var setResult = contentItem.SetTranslationTags(languageCode, resolvedTagIds, userId, now);
        if (setResult.IsFailure)
        {
            return setResult;
        }

        await contentRevisionRecorder.RecordAsync(contentItem, ContentItemRevisionKind.Edited, [languageCode], userId, now, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
