using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.ContentPaths;
using GenclikMerkezi.Modules.Website.Application.ContentRevisions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.RestoreContentItemRevision;

// ADR-024 §4 (Faz 5 Görev 7): restores only the selected language's captured fields (Title, Summary,
// Body, the six captured SEO sub-fields, TagIds - and, if asked, the content-level CategoryIds) back
// onto the live ContentItem. "Slug, durum, yol ve yönlendirmelere dokunulmaz": the existing slug is
// passed straight back into SetTranslation (slug/path recompute to the exact same value), and
// MetaKeywords/OgImageMediaId (never captured by a revision) are carried over unchanged from the
// current translation rather than cleared.
public sealed class RestoreContentItemRevisionCommandHandler(
    IContentItemRepository contentItemRepository,
    IContentTypeRepository contentTypeRepository,
    IContentItemRevisionRepository contentItemRevisionRepository,
    ContentPathCascadeService contentPathCascadeService,
    IHtmlContentSanitizer htmlContentSanitizer,
    ISearchIndexUpdater searchIndexUpdater,
    IContentRevisionRecorder contentRevisionRecorder,
    ICurrentUserContext currentUserContext,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<RestoreContentItemRevisionCommand, Result>
{
    public async Task<Result> Handle(RestoreContentItemRevisionCommand request, CancellationToken cancellationToken)
    {
        var contentItem = await contentItemRepository.GetByIdAsync(request.ContentItemId, cancellationToken);
        if (contentItem is null)
        {
            return Result.Failure(Error.NotFound("ContentItem.NotFound", $"Content item '{request.ContentItemId}' could not be found."));
        }

        if (!request.RowVersion!.SequenceEqual(contentItem.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.ConcurrencyConflict", "The content item was changed by someone else. Reload and try again."));
        }

        var trashCheck = ContentItemTrashGuard.EnsureEditable(contentItem);
        if (trashCheck.IsFailure)
        {
            return trashCheck;
        }

        var languageCodeResult = LanguageCode.Create(request.LanguageCode);
        if (languageCodeResult.IsFailure)
        {
            return languageCodeResult;
        }

        var languageCode = languageCodeResult.Value;

        var revision = await contentItemRevisionRepository.GetByRevisionNumberAsync(
            request.ContentItemId, request.RevisionNumber, cancellationToken);
        if (revision is null)
        {
            return Result.Failure(Error.NotFound(
                "ContentItemRevision.NotFound", $"Revision '{request.RevisionNumber}' could not be found for this content item."));
        }

        var snapshot = ContentItemRevisionSnapshotSerializer.Deserialize(revision.SnapshotJson);
        var snapshotTranslation = snapshot.Translations.FirstOrDefault(t => t.LanguageCode == languageCode.Value);
        if (snapshotTranslation is null)
        {
            return Result.Failure(Error.NotFound(
                "ContentItemRevision.TranslationNotFound", $"This revision has no snapshot for language '{languageCode}'."));
        }

        var currentTranslation = contentItem.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (currentTranslation is null)
        {
            return Result.Failure(Error.Conflict(
                "Revision.TranslationNoLongerExists",
                $"Language '{languageCode}' no longer has a translation on this content item; it is not recreated by a restore."));
        }

        var contentType = await contentTypeRepository.GetByIdAsync(contentItem.ContentTypeId, cancellationToken);
        if (contentType is null)
        {
            return Result.Failure(Error.Failure("ContentItem.ContentTypeNotFound", "The content item's content type could not be found."));
        }

        var contentTypeTranslation = contentType.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (contentTypeTranslation is null)
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.ContentTypeTranslationMissing", $"The content type has no translation for language '{languageCode}'."));
        }

        ContentItem? parent = null;
        if (contentItem.ParentId is not null)
        {
            parent = await contentItemRepository.GetByIdAsync(contentItem.ParentId.Value, cancellationToken);
        }

        var ancestorSlugs = await contentPathCascadeService.GetAncestorSlugsAsync(parent, languageCode, cancellationToken);

        var seoResult = SeoMetadata.Create(
            snapshotTranslation.MetaTitle, snapshotTranslation.MetaDescription, currentTranslation.Seo.MetaKeywords,
            snapshotTranslation.OgTitle, snapshotTranslation.OgDescription, currentTranslation.Seo.OgImageMediaId,
            snapshotTranslation.CanonicalUrl, snapshotTranslation.NoIndex);
        if (seoResult.IsFailure)
        {
            return seoResult;
        }

        var sanitizedBody = htmlContentSanitizer.Sanitize(snapshotTranslation.Body);
        var userId = currentUserContext.UserId!.Value;
        var now = DateTime.UtcNow;

        // currentTranslation.Slug is passed straight back in - slug/path never move during a restore.
        var setResult = contentItem.SetTranslation(
            languageCode, snapshotTranslation.Title, currentTranslation.Slug, contentTypeTranslation.RoutePrefix, ancestorSlugs,
            snapshotTranslation.Summary, sanitizedBody, seoResult.Value, userId, now);
        if (setResult.IsFailure)
        {
            return setResult;
        }

        var setTagsResult = contentItem.SetTranslationTags(languageCode, snapshotTranslation.TagIds, userId, now);
        if (setTagsResult.IsFailure)
        {
            return setTagsResult;
        }

        if (request.RestoreCategories)
        {
            var setCategoriesResult = contentItem.SetCategories(snapshot.CategoryIds, userId, now);
            if (setCategoriesResult.IsFailure)
            {
                return setCategoriesResult;
            }
        }

        await searchIndexUpdater.ReindexAsync(contentItem, cancellationToken);
        await contentRevisionRecorder.RecordAsync(
            contentItem, ContentItemRevisionKind.Restored, [languageCode], userId, now, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
