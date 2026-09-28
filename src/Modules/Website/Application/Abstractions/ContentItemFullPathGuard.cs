using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §4.3 (Faz 1a Görev 3): FullPath is unique per language across the whole module
// (IContentItemRepository.FullPathExistsAsync). When the owning ContentType's RoutePrefix is empty,
// FullPath is just the item's own slug, which is then the sole path segment - exactly the case
// RoutePrefix itself would otherwise guard (ReservedRouteSegments, and collision with another type's
// RoutePrefix), so that check applies here too, but only in that one case: a non-empty RoutePrefix was
// already checked once, at the type level, when the ContentType itself was created/updated.
public static class ContentItemFullPathGuard
{
    public static async Task<Result> CheckAsync(
        string fullPath,
        string routePrefix,
        string slug,
        LanguageCode languageCode,
        Guid? excludeContentItemId,
        ISiteLanguageRepository siteLanguageRepository,
        IContentTypeRepository contentTypeRepository,
        IContentItemRepository contentItemRepository,
        CancellationToken cancellationToken)
    {
        if (routePrefix.Length == 0)
        {
            var allLanguages = await siteLanguageRepository.GetAllAsync(cancellationToken);
            if (ReservedRouteSegments.IsReserved(slug, allLanguages.Select(l => l.Code.Value)))
            {
                return Result.Failure(Error.Conflict(
                    "ContentItem.SlugReserved", $"'{slug}' is a reserved route segment and cannot be used as a root-level slug."));
            }

            if (await contentTypeRepository.RoutePrefixExistsAsync(languageCode, slug, excludeId: null, cancellationToken))
            {
                return Result.Failure(Error.Conflict(
                    "ContentItem.SlugCollidesWithContentTypeRoutePrefix",
                    $"'{slug}' is already used as another content type's route prefix in language '{languageCode}'."));
            }
        }

        var fullPathExists = await contentItemRepository.FullPathExistsAsync(languageCode, fullPath, excludeContentItemId, cancellationToken);

        return fullPathExists
            ? Result.Failure(Error.Conflict(
                "ContentItem.FullPathAlreadyExists", $"'{fullPath}' is already used by another content item in language '{languageCode}'."))
            : Result.Success();
    }
}
