using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Popups;

// Faz 2 Görev 6 master prompt §6 "Paths ... dil öneki içermez": PopupTargeting itself cannot check
// this (it has no SiteLanguage access), so CreatePopupCommandHandler/UpdatePopupCommandHandler both
// call this shared guard instead - mirrors MediaImageReferenceGuard's own "small static Application
// guard used by multiple handlers" shape.
public static class PopupTargetingLanguagePrefixGuard
{
    public static async Task<Result> CheckAsync(
        PopupTargeting targeting, ISiteLanguageRepository siteLanguageRepository, CancellationToken cancellationToken)
    {
        if (targeting.Kind != PopupTargetingKind.Paths)
        {
            return Result.Success();
        }

        var languageCodes = (await siteLanguageRepository.GetAllAsync(cancellationToken))
            .Select(l => l.Code.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var path in targeting.Paths)
        {
            var (firstSegment, _) = RoutePathFormat.SplitFirstSegment(RoutePathFormat.Normalize(path));
            if (languageCodes.Contains(firstSegment))
            {
                return Result.Failure(Error.Validation(
                    "PopupTargeting.PathContainsLanguagePrefix", $"Path '{path}' must not start with a language code segment."));
            }
        }

        return Result.Success();
    }
}
