using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Popups;

// Shared by CreatePopup/UpdatePopup's command handlers: whether Targeting.Paths' language-prefix rule
// and Targeting.Contents' "every referenced content item still exists" rule hold - both need
// cross-aggregate repository access PopupTargeting itself does not have.
public static class PopupTargetingReferenceValidator
{
    public static async Task<Result> ValidateAsync(
        PopupTargeting targeting, ISiteLanguageRepository siteLanguageRepository, IContentItemRepository contentItemRepository,
        CancellationToken cancellationToken)
    {
        if (targeting.Kind == PopupTargetingKind.Paths)
        {
            var languageGuard = await PopupTargetingLanguagePrefixGuard.CheckAsync(targeting, siteLanguageRepository, cancellationToken);
            if (languageGuard.IsFailure)
            {
                return languageGuard;
            }
        }

        if (targeting.Kind == PopupTargetingKind.Contents)
        {
            var items = await contentItemRepository.GetByIdsAsync(targeting.ContentItemIds, cancellationToken);
            if (items.Count != targeting.ContentItemIds.Count)
            {
                return Result.Failure(Error.NotFound("Popup.ContentItemNotFound", "One or more targeted content items could not be found."));
            }
        }

        return Result.Success();
    }
}
