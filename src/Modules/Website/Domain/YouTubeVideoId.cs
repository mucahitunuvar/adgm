using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §5 (Faz 1b Görev 2): only YouTube is accepted. Every recognized URL shape is only ever used
// to extract the 11-character video ID - that ID is the only thing stored, so BuildEmbedUrl/
// BuildThumbnailUrl always regenerate a canonical, trusted URL rather than storing and replaying
// whatever the admin originally pasted.
public sealed partial class YouTubeVideoId : ValueObject
{
    public const int Length = 11;

    public string Value { get; }

    private YouTubeVideoId(string value)
    {
        Value = value;
    }

    // EF Core's value converter (VideoConfiguration) reconstructs from the stored, already-extracted
    // 11-character ID - not a URL - so it must not go through Create's URL parsing (which would reject
    // a bare ID as an invalid absolute URI). The stored value was already validated once, on write.
    internal static YouTubeVideoId FromStoredId(string id) => new(id);

    public static Result<YouTubeVideoId> Create(string? url)
    {
        var trimmed = (url ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            return Result.Failure<YouTubeVideoId>(Error.Validation("YouTubeVideoId.Required", "A YouTube URL is required."));
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return Invalid();
        }

        var host = uri.Host.ToLowerInvariant();
        if (host.StartsWith("www.", StringComparison.Ordinal))
        {
            host = host["www.".Length..];
        }

        var segments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

        string? candidateId = (host, segments) switch
        {
            ("youtu.be", [var id]) => id,
            ("youtube.com" or "m.youtube.com", ["watch"]) => GetQueryParameter(uri, "v"),
            ("youtube.com" or "m.youtube.com", ["embed", var id]) => id,
            ("youtube.com" or "m.youtube.com", ["shorts", var id]) => id,
            ("youtube-nocookie.com", ["embed", var id]) => id,
            _ => null,
        };

        if (candidateId is null || !VideoIdPattern().IsMatch(candidateId))
        {
            return Invalid();
        }

        return Result.Success(new YouTubeVideoId(candidateId));
    }

    public string BuildEmbedUrl() => $"https://www.youtube-nocookie.com/embed/{Value}";

    public string BuildThumbnailUrl() => $"https://i.ytimg.com/vi/{Value}/hqdefault.jpg";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    private static Result<YouTubeVideoId> Invalid() =>
        Result.Failure<YouTubeVideoId>(Error.Validation(
            "YouTubeVideoId.Invalid", "The URL is not a recognized YouTube video URL."));

    // No ASP.NET Core query-string helper is available to Domain (AGENTS.md §7) - a plain manual
    // split is enough for the one query parameter ("v") this type ever needs to read.
    private static string? GetQueryParameter(Uri uri, string name)
    {
        var query = uri.Query.TrimStart('?');
        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2 && parts[0] == name)
            {
                return Uri.UnescapeDataString(parts[1]);
            }
        }

        return null;
    }

    [GeneratedRegex("^[A-Za-z0-9_-]{11}$")]
    private static partial Regex VideoIdPattern();
}
