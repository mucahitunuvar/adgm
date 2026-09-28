using GenclikMerkezi.Modules.Website.Application.Abstractions;
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
    ICurrentUserContext currentUserContext,
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

        // Guaranteed by ContentType's own invariant (the default-language translation is mandatory and
        // can never be removed) - see ContentType.Create/RemoveTranslation.
        var routePrefix = contentType.Translations.First(t => t.LanguageCode == defaultLanguage.Code).RoutePrefix;

        var coverImageCheck = await MediaImageReferenceGuard.CheckAsync(request.CoverImageMediaId, "CoverImage", mediaAssetRepository, cancellationToken);
        if (coverImageCheck.IsFailure)
        {
            return Result.Failure<CreateContentItemResponse>(coverImageCheck.Error);
        }

        var detailImageCheck = await MediaImageReferenceGuard.CheckAsync(request.DetailImageMediaId, "DetailImage", mediaAssetRepository, cancellationToken);
        if (detailImageCheck.IsFailure)
        {
            return Result.Failure<CreateContentItemResponse>(detailImageCheck.Error);
        }

        var ogImageCheck = await MediaImageReferenceGuard.CheckAsync(request.Seo.OgImageMediaId, "SeoOgImage", mediaAssetRepository, cancellationToken);
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
            request.ContentTypeId, contentType.SupportsDetailImage, request.SortOrder, request.IsFeatured,
            request.CoverImageMediaId, request.DetailImageMediaId, defaultLanguage.Code, request.DefaultLanguageTitle,
            request.DefaultLanguageSlug, routePrefix, request.DefaultLanguageSummary, sanitizedBody, seoResult.Value,
            currentUserContext.UserId!.Value, DateTime.UtcNow);
        if (contentItemResult.IsFailure)
        {
            return Result.Failure<CreateContentItemResponse>(contentItemResult.Error);
        }

        var translation = contentItemResult.Value.Translations[0];
        var fullPathCheck = await ContentItemFullPathGuard.CheckAsync(
            translation.FullPath, routePrefix, translation.Slug, defaultLanguage.Code, excludeContentItemId: null,
            siteLanguageRepository, contentTypeRepository, contentItemRepository, cancellationToken);
        if (fullPathCheck.IsFailure)
        {
            return Result.Failure<CreateContentItemResponse>(fullPathCheck.Error);
        }

        contentItemRepository.Add(contentItemResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateContentItemResponse(contentItemResult.Value.Id, defaultLanguage.Code.Value, translation.FullPath));
    }
}
