using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// Faz 2 Görev 6 master prompt §6 "Hedefleme": where a popup is eligible to appear. Four mutually
// exclusive shapes - ContentItemIds is populated only for Contents, Paths only for Paths - the same
// "one VO, several factories, empty elsewhere" shape LinkTarget already uses. Whether a referenced
// content item still exists/is visible, and whether a path's first segment collides with a site
// language code ("dil öneki içermez"), both need cross-aggregate repository access this type does not
// have, so those checks stay in the Application-layer command handlers (mirrors LinkTarget's own split).
public sealed class PopupTargeting : ValueObject
{
    public const int MaxContentItemIds = 50;
    public const int MaxPaths = 20;
    public const int MaxPathLength = 500;

    public PopupTargetingKind Kind { get; }

    public IReadOnlyList<Guid> ContentItemIds { get; }

    public IReadOnlyList<string> Paths { get; }

    private PopupTargeting(PopupTargetingKind kind, IReadOnlyList<Guid> contentItemIds, IReadOnlyList<string> paths)
    {
        Kind = kind;
        ContentItemIds = contentItemIds;
        Paths = paths;
    }

    public static PopupTargeting CreateAllPages() => new(PopupTargetingKind.AllPages, [], []);

    public static PopupTargeting CreateHomeOnly() => new(PopupTargetingKind.HomeOnly, [], []);

    public static Result<PopupTargeting> CreateForContents(IReadOnlyList<Guid>? contentItemIds)
    {
        var distinct = (contentItemIds ?? []).Where(id => id != Guid.Empty).Distinct().ToList();

        if (distinct.Count == 0)
        {
            return Result.Failure<PopupTargeting>(Error.Validation(
                "PopupTargeting.ContentsRequired", "At least one content item id is required for 'Contents' targeting."));
        }

        if (distinct.Count > MaxContentItemIds)
        {
            return Result.Failure<PopupTargeting>(Error.Validation(
                "PopupTargeting.TooManyContents", $"Targeting can reference at most {MaxContentItemIds} content items."));
        }

        return Result.Success(new PopupTargeting(PopupTargetingKind.Contents, distinct, []));
    }

    public static Result<PopupTargeting> CreateForPaths(IReadOnlyList<string>? paths)
    {
        var normalized = new List<string>();

        foreach (var raw in paths ?? [])
        {
            var pathResult = NormalizePath(raw);
            if (pathResult.IsFailure)
            {
                return Result.Failure<PopupTargeting>(pathResult.Error);
            }

            normalized.Add(pathResult.Value);
        }

        if (normalized.Count == 0)
        {
            return Result.Failure<PopupTargeting>(Error.Validation(
                "PopupTargeting.PathsRequired", "At least one path is required for 'Paths' targeting."));
        }

        if (normalized.Count > MaxPaths)
        {
            return Result.Failure<PopupTargeting>(Error.Validation(
                "PopupTargeting.TooManyPaths", $"Targeting can reference at most {MaxPaths} paths."));
        }

        if (normalized.Distinct(StringComparer.Ordinal).Count() != normalized.Count)
        {
            return Result.Failure<PopupTargeting>(Error.Validation("PopupTargeting.DuplicatePath", "A path appears more than once."));
        }

        return Result.Success(new PopupTargeting(PopupTargetingKind.Paths, [], normalized));
    }

    // §6 "her biri tam yol ya da /* ile biten önek, örn. /haberler/*; dil öneki içermez" (the language-
    // prefix half is checked by the caller, which alone knows the site's configured languages).
    private static Result<string> NormalizePath(string? raw)
    {
        var value = (raw ?? string.Empty).Trim();

        if (value.Length == 0 || value.Length > MaxPathLength)
        {
            return Result.Failure<string>(Error.Validation(
                "PopupTargeting.PathInvalid", $"A path must be 1-{MaxPathLength} characters."));
        }

        if (!value.StartsWith('/') || value.Contains("//", StringComparison.Ordinal) || value.Contains('\\') || value.Contains(':'))
        {
            return Result.Failure<string>(Error.Validation(
                "PopupTargeting.PathInvalid", "A path must start with '/' and must not contain '//', '\\' or a scheme (':')."));
        }

        var starIndex = value.IndexOf('*');
        if (starIndex >= 0 && (starIndex != value.Length - 1 || value[starIndex - 1] != '/'))
        {
            return Result.Failure<string>(Error.Validation(
                "PopupTargeting.PathInvalid", "A wildcard path must end with '/*' and contain no other '*'."));
        }

        return Result.Success(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Kind;
        yield return string.Join(',', ContentItemIds);
        yield return string.Join(',', Paths);
    }
}
