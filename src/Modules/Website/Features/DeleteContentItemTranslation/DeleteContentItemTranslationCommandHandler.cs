using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeleteContentItemTranslation;

public sealed class DeleteContentItemTranslationCommandHandler(
    IContentItemRepository contentItemRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteContentItemTranslationCommand, Result>
{
    public async Task<Result> Handle(DeleteContentItemTranslationCommand request, CancellationToken cancellationToken)
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

        var trashCheck = ContentItemTrashGuard.EnsureEditable(contentItem);
        if (trashCheck.IsFailure)
        {
            return trashCheck;
        }

        var languageCodeResult = LanguageCode.Create(request.LanguageCode);
        if (languageCodeResult.IsFailure)
        {
            return languageCodeResult;
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure(Error.Failure("ContentItem.NoDefaultLanguage", "No default site language is configured."));
        }

        // ADR-024 §4.3 Görev 4: a child's translation can never outlive its parent's translation in
        // that same language.
        var children = await contentItemRepository.GetChildrenAsync(contentItem.Id, cancellationToken);
        var childrenWithLanguage = children.Count(c => c.Translations.Any(t => t.LanguageCode == languageCodeResult.Value));
        if (childrenWithLanguage > 0)
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.ChildrenStillHaveTranslation",
                $"Cannot delete: {childrenWithLanguage} child content item(s) still have a translation in language '{languageCodeResult.Value}'."));
        }

        var removeResult = contentItem.RemoveTranslation(
            languageCodeResult.Value, defaultLanguage.Code, currentUserContext.UserId!.Value, DateTime.UtcNow);
        if (removeResult.IsFailure)
        {
            return removeResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
