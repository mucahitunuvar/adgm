using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §15 (Faz 1a Görev 2, reused by Görev 4's root-level ContentItem path checks):
// ReservedRouteSegments (Domain) has no repository access, so checking a normalized RoutePrefix
// against SiteLanguage's actual codes, and against every OTHER ContentType's RoutePrefix in the same
// language, both happen here - the Application-layer half of the rule, called by every command that
// sets a ContentType's RoutePrefix (CreateContentType, UpdateContentTypeTranslation) so the check
// never drifts between them.
public static class RoutePrefixGuard
{
    public static async Task<Result> CheckAsync(
        string? routePrefix,
        LanguageCode languageCode,
        Guid? excludeContentTypeId,
        ISiteLanguageRepository siteLanguageRepository,
        IContentTypeRepository contentTypeRepository,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(routePrefix))
        {
            return Result.Success();
        }

        var normalizedResult = Slug.Create(routePrefix);
        if (normalizedResult.IsFailure)
        {
            return Result.Failure(normalizedResult.Error);
        }

        var normalized = normalizedResult.Value.Value;

        var allLanguages = await siteLanguageRepository.GetAllAsync(cancellationToken);
        if (ReservedRouteSegments.IsReserved(normalized, allLanguages.Select(l => l.Code.Value)))
        {
            return Result.Failure(Error.Conflict(
                "ContentType.RoutePrefixReserved",
                $"'{normalized}' is a reserved route segment and cannot be used as a route prefix."));
        }

        var exists = await contentTypeRepository.RoutePrefixExistsAsync(languageCode, normalized, excludeContentTypeId, cancellationToken);

        return exists
            ? Result.Failure(Error.Conflict(
                "ContentType.RoutePrefixAlreadyExists",
                $"'{normalized}' is already used as a route prefix by another content type in language '{languageCode}'."))
            : Result.Success();
    }
}
