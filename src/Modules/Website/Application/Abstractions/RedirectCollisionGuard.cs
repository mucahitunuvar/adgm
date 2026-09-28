using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §15 (Faz 1a Görev 5): the three collision rules a manually-created (or converted-from-a-
// 404) Redirect must pass, shared by CreateRedirect and ConvertNotFoundPathToRedirect so they can
// never drift apart - a live ContentItem's FullPath always wins, FromPath is unique, and a redirect
// chain (this one's target is itself another redirect's source) is rejected outright rather than
// followed or collapsed.
public static class RedirectCollisionGuard
{
    public static async Task<Result> CheckAsync(
        Redirect redirect,
        IContentItemRepository contentItemRepository,
        IRedirectRepository redirectRepository,
        CancellationToken cancellationToken)
    {
        if (await contentItemRepository.FullPathExistsAsync(redirect.LanguageCode, redirect.FromPath, excludeId: null, cancellationToken))
        {
            return Result.Failure(Error.Conflict(
                "Redirect.FromPathCollidesWithContentItem", $"'{redirect.FromPath}' is already used by a live content item."));
        }

        if (await redirectRepository.GetByFromPathAsync(redirect.LanguageCode, redirect.FromPath, cancellationToken) is not null)
        {
            return Result.Failure(Error.Conflict(
                "Redirect.FromPathAlreadyExists", $"A redirect from '{redirect.FromPath}' already exists in language '{redirect.LanguageCode}'."));
        }

        if (redirect.TargetKind == RedirectTargetKind.Path
            && await redirectRepository.GetByFromPathAsync(redirect.LanguageCode, redirect.TargetPath!, cancellationToken) is not null)
        {
            return Result.Failure(Error.Conflict(
                "Redirect.TargetWouldCreateChain",
                $"'{redirect.TargetPath}' is itself the source of another redirect; chained redirects are not supported."));
        }

        return Result.Success();
    }
}
