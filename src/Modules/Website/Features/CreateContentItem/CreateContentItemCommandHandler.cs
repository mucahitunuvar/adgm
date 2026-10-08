using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.ContentPaths;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.CreateContentItem;

public sealed class CreateContentItemCommandHandler(
    IContentItemRepository contentItemRepository,
    IContentTypeRepository contentTypeRepository,
    ISiteLanguageRepository siteLanguageRepository,
    IMediaAssetRepository mediaAssetRepository,
    IHtmlContentSanitizer htmlContentSanitizer,
    ContentPathCascadeService contentPathCascadeService,
    ISearchIndexUpdater searchIndexUpdater,
    IContentRevisionRecorder contentRevisionRecorder,
    ICurrentUserContext currentUserContext,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateContentItemCommand, Result<CreateContentItemResponse>>
{
    public async Task<Result<CreateContentItemResponse>> Handle(CreateContentItemCommand request, CancellationToken cancellationToken)
    {
        var contentType = await contentTypeRepository.GetByIdAsync(request.ContentTypeId, cancellationToken);
        if (contentType is null)
        {
            return Result.Failure<CreateContentItemResponse>(
                Error.NotFound("ContentItem.ContentTypeNotFound", $"Content type '{request.ContentTypeId}' could not be found."));
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure<CreateContentItemResponse>(
                Error.Failure("ContentItem.NoDefaultLanguage", "No default site language is configured."));
        }

        var parentCheck = await contentPathCascadeService.ValidateParentAsync(
            Guid.Empty, request.ContentTypeId, contentType.SupportsHierarchy, request.ParentId, cancellationToken);
        if (parentCheck.IsFailure)
        {
            return Result.Failure<CreateContentItemResponse>(parentCheck.Error);
        }

        var parent = parentCheck.Value;

        // A parent's translation in the default language is guaranteed to exist (SetTranslation
        // enforces "a child's language requires the parent to have it too"), the same way ContentType's
        // own default-language translation is guaranteed by ContentType.Create/RemoveTranslation.
        var routePrefix = contentType.Translations.First(t => t.LanguageCode == defaultLanguage.Code).RoutePrefix;
        var ancestorSlugs = await contentPathCascadeService.GetAncestorSlugsAsync(parent, defaultLanguage.Code, cancellationToken);

        var coverImageCheck = await MediaImageReferenceGuard.CheckAsync(request.CoverImageMediaId, "ContentItem", "CoverImage", mediaAssetRepository, cancellationToken);
        if (coverImageCheck.IsFailure)
        {
            return Result.Failure<CreateContentItemResponse>(coverImageCheck.Error);
        }

        var detailImageCheck = await MediaImageReferenceGuard.CheckAsync(request.DetailImageMediaId, "ContentItem", "DetailImage", mediaAssetRepository, cancellationToken);
        if (detailImageCheck.IsFailure)
        {
            return Result.Failure<CreateContentItemResponse>(detailImageCheck.Error);
        }

        var ogImageCheck = await MediaImageReferenceGuard.CheckAsync(request.Seo.OgImageMediaId, "ContentItem", "SeoOgImage", mediaAssetRepository, cancellationToken);
        if (ogImageCheck.IsFailure)
        {
            return Result.Failure<CreateContentItemResponse>(ogImageCheck.Error);
        }

        var seoResult = SeoMetadata.Create(
            request.Seo.MetaTitle, request.Seo.MetaDescription, request.Seo.MetaKeywords, request.Seo.OgTitle,
            request.Seo.OgDescription, request.Seo.OgImageMediaId, request.Seo.CanonicalUrl, request.Seo.NoIndex);
        if (seoResult.IsFailure)
        {
            return Result.Failure<CreateContentItemResponse>(seoResult.Error);
        }

        var sanitizedBody = htmlContentSanitizer.Sanitize(request.DefaultLanguageBody ?? string.Empty);

        var contentItemResult = ContentItem.Create(
            request.ContentTypeId, request.ParentId, contentType.SupportsDetailImage, request.SortOrder, request.IsFeatured,
            request.CoverImageMediaId, request.DetailImageMediaId, defaultLanguage.Code, request.DefaultLanguageTitle,
            request.DefaultLanguageSlug, routePrefix, ancestorSlugs, request.DefaultLanguageSummary, sanitizedBody,
            seoResult.Value, currentUserContext.UserId!.Value, DateTime.UtcNow);
        if (contentItemResult.IsFailure)
        {
            return Result.Failure<CreateContentItemResponse>(contentItemResult.Error);
        }

        var translation = contentItemResult.Value.Translations[0];
        var fullPathCheck = await ContentItemFullPathGuard.CheckAsync(
            translation.FullPath, routePrefix, translation.Slug, request.ParentId, defaultLanguage.Code, excludeContentItemId: null,
            siteLanguageRepository, contentTypeRepository, contentItemRepository, cancellationToken);
        if (fullPathCheck.IsFailure)
        {
            return Result.Failure<CreateContentItemResponse>(fullPathCheck.Error);
        }

        contentItemRepository.Add(contentItemResult.Value);
        await searchIndexUpdater.ReindexAsync(contentItemResult.Value, cancellationToken);

        // ADR-024 §4 (Faz 5 Görev 7): this item's very first revision - always recorded, never
        // hash-deduped (there is nothing earlier to compare against).
        await contentRevisionRecorder.RecordAsync(
            contentItemResult.Value, ContentItemRevisionKind.Created, [defaultLanguage.Code], currentUserContext.UserId!.Value,
            DateTime.UtcNow, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success(new CreateContentItemResponse(contentItemResult.Value.Id, defaultLanguage.Code.Value, translation.FullPath));
    }
}
