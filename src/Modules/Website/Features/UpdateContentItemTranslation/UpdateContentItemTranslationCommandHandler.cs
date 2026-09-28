using GenclikMerkezi.Modules.Website.Application.Abstractions;
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

        var contentType = await contentTypeRepository.GetByIdAsync(contentItem.ContentTypeId, cancellationToken);
        if (contentType is null)
        {
            return Result.Failure(Error.Failure("ContentItem.ContentTypeNotFound", "The content item's content type could not be found."));
        }

        var contentTypeTranslation = contentType.Translations.FirstOrDefault(t => t.LanguageCode == languageCodeResult.Value);
        if (contentTypeTranslation is null)
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.ContentTypeTranslationMissing",
                $"The content type has no translation for language '{languageCodeResult.Value}' yet; add that first."));
        }

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

        var setResult = contentItem.SetTranslation(
            languageCodeResult.Value, request.Title, request.Slug, contentTypeTranslation.RoutePrefix, request.Summary,
            sanitizedBody, seoResult.Value, currentUserContext.UserId!.Value, DateTime.UtcNow);
        if (setResult.IsFailure)
        {
            return setResult;
        }

        var translation = contentItem.Translations.First(t => t.LanguageCode == languageCodeResult.Value);
        var fullPathCheck = await ContentItemFullPathGuard.CheckAsync(
            translation.FullPath, contentTypeTranslation.RoutePrefix, translation.Slug, languageCodeResult.Value, contentItem.Id,
            siteLanguageRepository, contentTypeRepository, contentItemRepository, cancellationToken);
        if (fullPathCheck.IsFailure)
        {
            return fullPathCheck;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
