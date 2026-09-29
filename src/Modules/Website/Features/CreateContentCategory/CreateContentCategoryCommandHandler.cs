using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.CreateContentCategory;

public sealed class CreateContentCategoryCommandHandler(
    IContentCategoryRepository contentCategoryRepository,
    IContentTypeRepository contentTypeRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateContentCategoryCommand, Result<CreateContentCategoryResponse>>
{
    public async Task<Result<CreateContentCategoryResponse>> Handle(CreateContentCategoryCommand request, CancellationToken cancellationToken)
    {
        var contentType = await contentTypeRepository.GetByIdAsync(request.ContentTypeId, cancellationToken);
        if (contentType is null)
        {
            return Result.Failure<CreateContentCategoryResponse>(
                Error.NotFound("ContentType.NotFound", $"Content type '{request.ContentTypeId}' could not be found."));
        }

        if (!contentType.SupportsCategories)
        {
            return Result.Failure<CreateContentCategoryResponse>(Error.Validation(
                "ContentCategory.ContentTypeDoesNotSupportCategories", "This content type does not support categories."));
        }

        if (request.ParentId is not null)
        {
            var parent = await contentCategoryRepository.GetByIdAsync(request.ParentId.Value, cancellationToken);
            if (parent is null || parent.ContentTypeId != request.ContentTypeId)
            {
                return Result.Failure<CreateContentCategoryResponse>(Error.NotFound(
                    "ContentCategory.ParentNotFound", $"Parent category '{request.ParentId}' could not be found for this content type."));
            }

            // ADR-024 §4.1: at most 2 levels - a category whose own parent is set cannot be used as a
            // parent itself.
            if (parent.ParentId is not null)
            {
                return Result.Failure<CreateContentCategoryResponse>(Error.Validation(
                    "ContentCategory.MaxDepthExceeded", "Categories can be at most 2 levels deep; the selected parent is already a sub-category."));
            }
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure<CreateContentCategoryResponse>(
                Error.Failure("ContentCategory.NoDefaultLanguage", "No default site language is configured."));
        }

        var slugCheck = await ContentCategorySlugGuard.CheckAsync(
            request.DefaultLanguageSlug, request.DefaultLanguageName, request.ContentTypeId, defaultLanguage.Code, null,
            contentCategoryRepository, cancellationToken);
        if (slugCheck.IsFailure)
        {
            return Result.Failure<CreateContentCategoryResponse>(slugCheck.Error);
        }

        var seoResult = SeoMetadata.Create(
            request.Seo.MetaTitle, request.Seo.MetaDescription, request.Seo.MetaKeywords, request.Seo.OgTitle,
            request.Seo.OgDescription, request.Seo.OgImageMediaId, request.Seo.CanonicalUrl, request.Seo.NoIndex);
        if (seoResult.IsFailure)
        {
            return Result.Failure<CreateContentCategoryResponse>(seoResult.Error);
        }

        var categoryResult = ContentCategory.Create(
            request.ContentTypeId, request.ParentId, request.SortOrder, defaultLanguage.Code, request.DefaultLanguageName,
            request.DefaultLanguageSlug, seoResult.Value, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (categoryResult.IsFailure)
        {
            return Result.Failure<CreateContentCategoryResponse>(categoryResult.Error);
        }

        contentCategoryRepository.Add(categoryResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateContentCategoryResponse(categoryResult.Value.Id, defaultLanguage.Code.Value));
    }
}
