using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateContentCategoryTranslation;

public sealed class UpdateContentCategoryTranslationCommandHandler(
    IContentCategoryRepository contentCategoryRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateContentCategoryTranslationCommand, Result>
{
    public async Task<Result> Handle(UpdateContentCategoryTranslationCommand request, CancellationToken cancellationToken)
    {
        var category = await contentCategoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null || category.ContentTypeId != request.TypeId)
        {
            return Result.Failure(Error.NotFound("ContentCategory.NotFound", $"Category '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(category.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "ContentCategory.ConcurrencyConflict", "The category was changed by someone else. Reload and try again."));
        }

        var languageCodeResult = LanguageCode.Create(request.LanguageCode);
        if (languageCodeResult.IsFailure)
        {
            return languageCodeResult;
        }

        var slugCheck = await ContentCategorySlugGuard.CheckAsync(
            request.Slug, request.Name, category.ContentTypeId, languageCodeResult.Value, category.Id,
            contentCategoryRepository, cancellationToken);
        if (slugCheck.IsFailure)
        {
            return slugCheck;
        }

        var seoResult = SeoMetadata.Create(
            request.Seo.MetaTitle, request.Seo.MetaDescription, request.Seo.MetaKeywords, request.Seo.OgTitle,
            request.Seo.OgDescription, request.Seo.OgImageMediaId, request.Seo.CanonicalUrl, request.Seo.NoIndex);
        if (seoResult.IsFailure)
        {
            return seoResult;
        }

        var setResult = category.SetTranslation(
            languageCodeResult.Value, request.Name, request.Slug, seoResult.Value,
            currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (setResult.IsFailure)
        {
            return setResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
