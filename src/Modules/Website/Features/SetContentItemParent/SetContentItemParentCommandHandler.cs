using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.ContentPaths;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.SetContentItemParent;

public sealed class SetContentItemParentCommandHandler(
    IContentItemRepository contentItemRepository,
    IContentTypeRepository contentTypeRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ContentPathCascadeService contentPathCascadeService,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<SetContentItemParentCommand, Result>
{
    public async Task<Result> Handle(SetContentItemParentCommand request, CancellationToken cancellationToken)
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

        if (contentItem.ParentId == request.ParentId)
        {
            return Result.Success();
        }

        var contentType = await contentTypeRepository.GetByIdAsync(contentItem.ContentTypeId, cancellationToken);
        if (contentType is null)
        {
            return Result.Failure(Error.Failure("ContentItem.ContentTypeNotFound", "The content item's content type could not be found."));
        }

        var parentCheck = await contentPathCascadeService.ValidateParentAsync(
            contentItem.Id, contentItem.ContentTypeId, contentType.SupportsHierarchy, request.ParentId, cancellationToken);
        if (parentCheck.IsFailure)
        {
            return parentCheck;
        }

        var newParent = parentCheck.Value;

        // Every language the item currently has must remain valid under the new parent too - a child's
        // translation can never exist in a language its parent lacks (the same rule
        // UpdateContentItemTranslation enforces when adding a translation).
        if (newParent is not null)
        {
            var missingLanguage = contentItem.Translations
                .Select(t => t.LanguageCode)
                .FirstOrDefault(languageCode => newParent.Translations.All(t => t.LanguageCode != languageCode));
            if (missingLanguage is not null)
            {
                return Result.Failure(Error.Conflict(
                    "ContentItem.ParentTranslationMissing",
                    $"The new parent has no translation for language '{missingLanguage}', which this item already has."));
            }
        }

        var userId = currentUserContext.UserId!.Value;
        var now = DateTime.UtcNow;

        foreach (var languageCode in contentItem.Translations.Select(t => t.LanguageCode).ToList())
        {
            var oldFullPath = contentItem.Translations.First(t => t.LanguageCode == languageCode).FullPath;
            var routePrefix = contentType.Translations.First(t => t.LanguageCode == languageCode).RoutePrefix;
            var ancestorSlugs = await contentPathCascadeService.GetAncestorSlugsAsync(newParent, languageCode, cancellationToken);

            contentItem.RecomputeFullPath(languageCode, routePrefix, ancestorSlugs, userId, now);
            var newFullPath = contentItem.Translations.First(t => t.LanguageCode == languageCode).FullPath;

            var fullPathCheck = await ContentItemFullPathGuard.CheckAsync(
                newFullPath, routePrefix, contentItem.Translations.First(t => t.LanguageCode == languageCode).Slug, request.ParentId,
                languageCode, contentItem.Id, siteLanguageRepository, contentTypeRepository, contentItemRepository, cancellationToken);
            if (fullPathCheck.IsFailure)
            {
                return fullPathCheck;
            }

            if (oldFullPath != newFullPath)
            {
                var redirectResult = await contentPathCascadeService.CreateAutomaticRedirectAsync(
                    languageCode, oldFullPath, newFullPath, contentItem.Id, userId, now, cancellationToken);
                if (redirectResult.IsFailure)
                {
                    return redirectResult;
                }

                var cascadeResult = await contentPathCascadeService.CascadeDescendantPathsAsync(
                    contentItem, languageCode, routePrefix, ancestorSlugs, userId, now, cancellationToken);
                if (cascadeResult.IsFailure)
                {
                    return cascadeResult;
                }
            }
        }

        contentItem.SetParent(request.ParentId, userId, now);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
