using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.ContentPaths;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentItemTranslation;

public sealed class UpdateContentItemTranslationCommandHandler(
    IContentItemRepository contentItemRepository,
    IContentTypeRepository contentTypeRepository,
    ISiteLanguageRepository siteLanguageRepository,
    IMediaAssetRepository mediaAssetRepository,
    IHtmlContentSanitizer htmlContentSanitizer,
    ContentPathCascadeService contentPathCascadeService,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateContentItemTranslationCommand, Result>
{
    public async Task<Result> Handle(UpdateContentItemTranslationCommand request, CancellationToken cancellationToken)
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

        var languageCodeResult = LanguageCode.Create(request.LanguageCode);
        if (languageCodeResult.IsFailure)
        {
            return languageCodeResult;
        }

        var languageCode = languageCodeResult.Value;

        var contentType = await contentTypeRepository.GetByIdAsync(contentItem.ContentTypeId, cancellationToken);
        if (contentType is null)
        {
            return Result.Failure(Error.Failure("ContentItem.ContentTypeNotFound", "The content item's content type could not be found."));
        }

        var contentTypeTranslation = contentType.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (contentTypeTranslation is null)
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.ContentTypeTranslationMissing",
                $"The content type has no translation for language '{languageCode}' yet; add that first."));
        }

        ContentItem? parent = null;
        if (contentItem.ParentId is not null)
        {
            parent = await contentItemRepository.GetByIdAsync(contentItem.ParentId.Value, cancellationToken);
            if (parent is null || parent.Translations.All(t => t.LanguageCode != languageCode))
            {
                return Result.Failure(Error.Conflict(
                    "ContentItem.ParentTranslationMissing",
                    $"The parent content item has no translation for language '{languageCode}' yet; add that first."));
            }
        }

        var ancestorSlugs = await contentPathCascadeService.GetAncestorSlugsAsync(parent, languageCode, cancellationToken);

        var ogImageCheck = await MediaImageReferenceGuard.CheckAsync(request.Seo.OgImageMediaId, "SeoOgImage", mediaAssetRepository, cancellationToken);
        if (ogImageCheck.IsFailure)
        {
            return ogImageCheck;
        }

        var seoResult = SeoMetadata.Create(
            request.Seo.MetaTitle, request.Seo.MetaDescription, request.Seo.MetaKeywords, request.Seo.OgTitle,
            request.Seo.OgDescription, request.Seo.OgImageMediaId, request.Seo.CanonicalUrl, request.Seo.NoIndex);
        if (seoResult.IsFailure)
        {
            return seoResult;
        }

        var sanitizedBody = htmlContentSanitizer.Sanitize(request.Body ?? string.Empty);
        var previousFullPath = contentItem.Translations.FirstOrDefault(t => t.LanguageCode == languageCode)?.FullPath;
        var now = DateTime.UtcNow;
        var userId = currentUserContext.UserId!.Value;

        var setResult = contentItem.SetTranslation(
            languageCode, request.Title, request.Slug, contentTypeTranslation.RoutePrefix, ancestorSlugs, request.Summary,
            sanitizedBody, seoResult.Value, userId, now);
        if (setResult.IsFailure)
        {
            return setResult;
        }

        var translation = contentItem.Translations.First(t => t.LanguageCode == languageCode);
        var fullPathCheck = await ContentItemFullPathGuard.CheckAsync(
            translation.FullPath, contentTypeTranslation.RoutePrefix, translation.Slug, contentItem.ParentId, languageCode,
            contentItem.Id, siteLanguageRepository, contentTypeRepository, contentItemRepository, cancellationToken);
        if (fullPathCheck.IsFailure)
        {
            return fullPathCheck;
        }

        if (previousFullPath is not null && previousFullPath != translation.FullPath)
        {
            var redirectResult = await contentPathCascadeService.CreateAutomaticRedirectAsync(
                languageCode, previousFullPath, translation.FullPath, contentItem.Id, userId, now, cancellationToken);
            if (redirectResult.IsFailure)
            {
                return redirectResult;
            }

            var cascadeResult = await contentPathCascadeService.CascadeDescendantPathsAsync(
                contentItem, languageCode, contentTypeTranslation.RoutePrefix, ancestorSlugs, userId, now, cancellationToken);
            if (cascadeResult.IsFailure)
            {
                return cascadeResult;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
