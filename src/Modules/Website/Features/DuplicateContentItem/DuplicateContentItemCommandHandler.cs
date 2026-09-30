using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.ContentPaths;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DuplicateContentItem;

// ADR-024 §4.5 (Faz 1b Görev 6): copies a content item into a new Draft, same type and parent, with
// every translation, image, gallery item, video, attachment, category, tag and manually linked related
// content item - but not children, scheduling or publish state. Each language gets its own new slug
// ("{slug}-kopya"/"{slug}-copy", retried with "-2", "-3"... on conflict) and title suffix
// (ContentItemDuplicateSuffixes is the single place both suffix texts are defined).
public sealed class DuplicateContentItemCommandHandler(
    IContentItemRepository contentItemRepository,
    IContentTypeRepository contentTypeRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ContentPathCascadeService contentPathCascadeService,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DuplicateContentItemCommand, Result<DuplicateContentItemResponse>>
{
    private const int MaxSlugAttempts = 50;

    public async Task<Result<DuplicateContentItemResponse>> Handle(DuplicateContentItemCommand request, CancellationToken cancellationToken)
    {
        var source = await contentItemRepository.GetByIdAsync(request.Id, cancellationToken);
        if (source is null)
        {
            return Result.Failure<DuplicateContentItemResponse>(
                Error.NotFound("ContentItem.NotFound", $"Content item '{request.Id}' could not be found."));
        }

        if (source.DeletedAtUtc is not null)
        {
            return Result.Failure<DuplicateContentItemResponse>(Error.Conflict(
                "ContentItem.CannotDuplicateTrashedContent", "A content item in the trash cannot be duplicated; restore it first."));
        }

        var contentType = await contentTypeRepository.GetByIdAsync(source.ContentTypeId, cancellationToken);
        if (contentType is null)
        {
            return Result.Failure<DuplicateContentItemResponse>(
                Error.Failure("ContentItem.ContentTypeNotFound", "The content item's content type could not be found."));
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure<DuplicateContentItemResponse>(
                Error.Failure("ContentItem.NoDefaultLanguage", "No default site language is configured."));
        }

        ContentItem? parent = null;
        if (source.ParentId is not null)
        {
            parent = await contentItemRepository.GetByIdAsync(source.ParentId.Value, cancellationToken);
        }

        var slugSuffixWord = ContentItemDuplicateSuffixes.SlugSuffixWord(defaultLanguage.Code);
        var userId = currentUserContext.UserId!.Value;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var defaultTranslation = source.Translations.First(t => t.LanguageCode == defaultLanguage.Code);
        var defaultRoutePrefix = contentType.Translations.First(t => t.LanguageCode == defaultLanguage.Code).RoutePrefix;
        var defaultAncestorSlugs = await contentPathCascadeService.GetAncestorSlugsAsync(parent, defaultLanguage.Code, cancellationToken);

        ContentItem? newItem = null;
        for (var attempt = 1; attempt <= MaxSlugAttempts; attempt++)
        {
            var candidateSlug = BuildCandidateSlug(defaultTranslation.Slug, slugSuffixWord, attempt);
            var createResult = ContentItem.Create(
                source.ContentTypeId, source.ParentId, contentType.SupportsDetailImage, source.SortOrder, source.IsFeatured,
                source.CoverImageMediaId, source.DetailImageMediaId, defaultLanguage.Code,
                defaultTranslation.Title + ContentItemDuplicateSuffixes.TitleSuffix(defaultLanguage.Code), candidateSlug,
                defaultRoutePrefix, defaultAncestorSlugs, defaultTranslation.Summary, defaultTranslation.Body, CopySeo(defaultTranslation.Seo),
                userId, now);
            if (createResult.IsFailure)
            {
                return Result.Failure<DuplicateContentItemResponse>(createResult.Error);
            }

            var candidateTranslation = createResult.Value.Translations[0];
            var fullPathCheck = await ContentItemFullPathGuard.CheckAsync(
                candidateTranslation.FullPath, defaultRoutePrefix, candidateTranslation.Slug, source.ParentId, defaultLanguage.Code,
                excludeContentItemId: null, siteLanguageRepository, contentTypeRepository, contentItemRepository, cancellationToken);
            if (fullPathCheck.IsSuccess)
            {
                newItem = createResult.Value;
                break;
            }

            if (fullPathCheck.Error.Code != "ContentItem.FullPathAlreadyExists")
            {
                return Result.Failure<DuplicateContentItemResponse>(fullPathCheck.Error);
            }
        }

        if (newItem is null)
        {
            return Result.Failure<DuplicateContentItemResponse>(Error.Conflict(
                "ContentItem.DuplicateSlugExhausted", "Could not find a free address for the duplicate after many attempts."));
        }

        foreach (var sourceTranslation in source.Translations.Where(t => t.LanguageCode != defaultLanguage.Code))
        {
            var languageCode = sourceTranslation.LanguageCode;
            var routePrefix = contentType.Translations.First(t => t.LanguageCode == languageCode).RoutePrefix;
            var languageAncestorSlugs = await contentPathCascadeService.GetAncestorSlugsAsync(parent, languageCode, cancellationToken);
            var titleSuffix = ContentItemDuplicateSuffixes.TitleSuffix(languageCode);

            var added = false;
            for (var attempt = 1; attempt <= MaxSlugAttempts; attempt++)
            {
                var candidateSlug = BuildCandidateSlug(sourceTranslation.Slug, slugSuffixWord, attempt);
                var setResult = newItem.SetTranslation(
                    languageCode, sourceTranslation.Title + titleSuffix, candidateSlug, routePrefix, languageAncestorSlugs,
                    sourceTranslation.Summary, sourceTranslation.Body, CopySeo(sourceTranslation.Seo), userId, now);
                if (setResult.IsFailure)
                {
                    return Result.Failure<DuplicateContentItemResponse>(setResult.Error);
                }

                var newTranslation = newItem.Translations.First(t => t.LanguageCode == languageCode);
                var fullPathCheck = await ContentItemFullPathGuard.CheckAsync(
                    newTranslation.FullPath, routePrefix, newTranslation.Slug, source.ParentId, languageCode, excludeContentItemId: null,
                    siteLanguageRepository, contentTypeRepository, contentItemRepository, cancellationToken);
                if (fullPathCheck.IsSuccess)
                {
                    added = true;
                    break;
                }

                if (fullPathCheck.Error.Code != "ContentItem.FullPathAlreadyExists")
                {
                    return Result.Failure<DuplicateContentItemResponse>(fullPathCheck.Error);
                }
            }

            if (!added)
            {
                return Result.Failure<DuplicateContentItemResponse>(Error.Conflict(
                    "ContentItem.DuplicateSlugExhausted", "Could not find a free address for the duplicate after many attempts."));
            }

            newItem.SetTranslationTags(languageCode, sourceTranslation.TagIds, userId, now);
        }

        newItem.SetTranslationTags(defaultLanguage.Code, defaultTranslation.TagIds, userId, now);
        newItem.SetCategories(source.CategoryIds, userId, now);
        newItem.SetVideos(source.VideoIds, userId, now);
        newItem.SetRelatedContent(source.RelatedContentItemIds, userId, now);

        var galleryItems = source.GalleryItems
            .Select(g => ContentItemGalleryItem.Create(
                g.MediaAssetId, g.SortOrder,
                g.Translations.Select(t => ContentItemGalleryItemTranslation.Create(t.LanguageCode, t.AltTextOverride, t.CaptionOverride).Value).ToList()))
            .ToList();
        newItem.SetGallery(galleryItems, userId, now);

        var attachments = source.Attachments
            .Select(a => ContentItemAttachment.Create(
                a.MediaAssetId, a.SortOrder,
                a.Translations.Select(t => ContentItemAttachmentTranslation.Create(t.LanguageCode, t.DisplayNameOverride).Value).ToList()))
            .ToList();
        newItem.SetAttachments(attachments, userId, now);

        contentItemRepository.Add(newItem);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success(new DuplicateContentItemResponse(newItem.Id));
    }

    private static string BuildCandidateSlug(string originalSlug, string suffixWord, int attempt) =>
        attempt == 1 ? $"{originalSlug}-{suffixWord}" : $"{originalSlug}-{suffixWord}-{attempt}";

    // SeoMetadata is an owned type keyed by its owning ContentItemTranslation's id - reusing the
    // source translation's own SeoMetadata instance on the NEW translation makes EF Core's change
    // tracker think the same owned row is being reassigned to a different owner ("part of a key and so
    // cannot be modified"). A fresh instance with the same (already-valid) field values avoids that
    // entirely; re-running Create's validation on already-valid data is a no-op.
    private static SeoMetadata CopySeo(SeoMetadata source) =>
        SeoMetadata.Create(
            source.MetaTitle, source.MetaDescription, source.MetaKeywords, source.OgTitle, source.OgDescription,
            source.OgImageMediaId, source.CanonicalUrl, source.NoIndex).Value;
}
