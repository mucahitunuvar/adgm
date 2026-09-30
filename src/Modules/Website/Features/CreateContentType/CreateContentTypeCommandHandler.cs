using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.CreateContentType;

public sealed class CreateContentTypeCommandHandler(
    IContentTypeRepository contentTypeRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateContentTypeCommand, Result<CreateContentTypeResponse>>
{
    public async Task<Result<CreateContentTypeResponse>> Handle(CreateContentTypeCommand request, CancellationToken cancellationToken)
    {
        var keyResult = ContentTypeKey.Create(request.Key);
        if (keyResult.IsFailure)
        {
            return Result.Failure<CreateContentTypeResponse>(keyResult.Error);
        }

        var existingByKey = await contentTypeRepository.GetByKeyAsync(keyResult.Value, cancellationToken);
        if (existingByKey is not null)
        {
            return Result.Failure<CreateContentTypeResponse>(Error.Conflict(
                "ContentType.KeyAlreadyExists", $"A content type with key '{keyResult.Value}' already exists."));
        }

        if (!Enum.TryParse<ContentTypeSortMode>(request.SortMode, ignoreCase: true, out var sortMode))
        {
            return Result.Failure<CreateContentTypeResponse>(Error.Validation(
                "ContentType.InvalidSortMode", $"'{request.SortMode}' is not a recognized sort mode."));
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure<CreateContentTypeResponse>(Error.Failure(
                "ContentType.NoDefaultLanguage", "No default site language is configured."));
        }

        var routePrefixCheck = await RoutePrefixGuard.CheckAsync(
            request.DefaultLanguageRoutePrefix, defaultLanguage.Code, excludeContentTypeId: null,
            siteLanguageRepository, contentTypeRepository, cancellationToken);
        if (routePrefixCheck.IsFailure)
        {
            return Result.Failure<CreateContentTypeResponse>(routePrefixCheck.Error);
        }

        var seoResult = SeoMetadata.Create(
            request.Seo.MetaTitle, request.Seo.MetaDescription, request.Seo.MetaKeywords, request.Seo.OgTitle,
            request.Seo.OgDescription, request.Seo.OgImageMediaId, request.Seo.CanonicalUrl, request.Seo.NoIndex);
        if (seoResult.IsFailure)
        {
            return Result.Failure<CreateContentTypeResponse>(seoResult.Error);
        }

        var flags = new ContentTypeFeatureFlags(
            request.SupportsHierarchy, request.SupportsCategories, request.SupportsTags, request.SupportsDetailImage,
            request.SupportsGallery, request.SupportsVideos, request.SupportsAttachments, request.SupportsEvent,
            request.SupportsBlockLayout, request.SupportsForm, request.SupportsRelatedContent, request.HasDetailPage,
            request.HasListingPage, request.IsSearchable, request.RequiresReview);

        var contentTypeResult = ContentType.Create(
            keyResult.Value, request.ListTemplate, request.DetailTemplate, sortMode, request.SortOrder, flags,
            defaultLanguage.Code, request.DefaultLanguageName, request.DefaultLanguageRoutePrefix, seoResult.Value,
            currentUserContext.UserId!.Value, DateTime.UtcNow);
        if (contentTypeResult.IsFailure)
        {
            return Result.Failure<CreateContentTypeResponse>(contentTypeResult.Error);
        }

        contentTypeRepository.Add(contentTypeResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidatePublicContent(cacheService);

        return Result.Success(new CreateContentTypeResponse(
            contentTypeResult.Value.Id, contentTypeResult.Value.Key.Value, defaultLanguage.Code.Value));
    }
}
