using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeleteContentCategoryTranslation;

public sealed class DeleteContentCategoryTranslationCommandHandler(
    IContentCategoryRepository contentCategoryRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteContentCategoryTranslationCommand, Result>
{
    public async Task<Result> Handle(DeleteContentCategoryTranslationCommand request, CancellationToken cancellationToken)
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

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure(Error.Failure("ContentCategory.NoDefaultLanguage", "No default site language is configured."));
        }

        var removeResult = category.RemoveTranslation(
            languageCodeResult.Value, defaultLanguage.Code, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (removeResult.IsFailure)
        {
            return removeResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
