using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// Faz 2 Görev 1 master prompt §1.1: shared by Menu (this Görev), Slider and Popup (Görev 2/6) - a
// single "where does this link go" concept with four mutually exclusive shapes plus an empty
// ("no link") sentinel, resolved to a public href by the Application-layer LinkTargetResolver (this
// type only validates its own shape; whether a Content/ContentTypeListing target actually resolves to
// something visible is a cross-aggregate concern that type cannot check alone).
public sealed class LinkTarget : ValueObject
{
    public const int MaxInternalPathLength = 500;
    public const int MaxExternalUrlLength = 1000;

    private static readonly string[] AllowedExternalUrlSchemes = ["https", "http", "mailto", "tel"];

    public LinkTargetKind Kind { get; }

    public Guid? ContentItemId { get; }

    public Guid? ContentTypeId { get; }

    public string? InternalPath { get; }

    public string? ExternalUrl { get; }

    private LinkTarget(LinkTargetKind kind, Guid? contentItemId, Guid? contentTypeId, string? internalPath, string? externalUrl)
    {
        Kind = kind;
        ContentItemId = contentItemId;
        ContentTypeId = contentTypeId;
        InternalPath = internalPath;
        ExternalUrl = externalUrl;
    }

    public bool IsEmpty => Kind == LinkTargetKind.None;

    public static LinkTarget CreateEmpty() => new(LinkTargetKind.None, null, null, null, null);

    public static Result<LinkTarget> ForContent(Guid contentItemId)
    {
        if (contentItemId == Guid.Empty)
        {
            return Result.Failure<LinkTarget>(Error.Validation("LinkTarget.ContentItemIdRequired", "A content item id is required."));
        }

        return Result.Success(new LinkTarget(LinkTargetKind.Content, contentItemId, null, null, null));
    }

    public static Result<LinkTarget> ForContentTypeListing(Guid contentTypeId)
    {
        if (contentTypeId == Guid.Empty)
        {
            return Result.Failure<LinkTarget>(Error.Validation("LinkTarget.ContentTypeIdRequired", "A content type id is required."));
        }

        return Result.Success(new LinkTarget(LinkTargetKind.ContentTypeListing, null, contentTypeId, null, null));
    }

    public static Result<LinkTarget> ForInternalPath(string? path)
    {
        var value = (path ?? string.Empty).Trim();

        if (value.Length == 0 || value.Length > MaxInternalPathLength)
        {
            return Result.Failure<LinkTarget>(Error.Validation(
                "LinkTarget.InternalPathInvalid", $"Internal path must be 1-{MaxInternalPathLength} characters."));
        }

        if (!value.StartsWith('/') || value.Contains("//", StringComparison.Ordinal) || value.Contains('\\') || value.Contains(':'))
        {
            return Result.Failure<LinkTarget>(Error.Validation(
                "LinkTarget.InternalPathInvalid",
                "Internal path must start with '/' and must not contain '//', '\\' or a scheme (':')."));
        }

        return Result.Success(new LinkTarget(LinkTargetKind.InternalPath, null, null, value, null));
    }

    public static Result<LinkTarget> ForExternalUrl(string? url)
    {
        var value = (url ?? string.Empty).Trim();

        if (value.Length == 0 || value.Length > MaxExternalUrlLength)
        {
            return Result.Failure<LinkTarget>(Error.Validation(
                "LinkTarget.ExternalUrlInvalid", $"External URL must be 1-{MaxExternalUrlLength} characters."));
        }

        var isAllowedAbsoluteUrl = Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && AllowedExternalUrlSchemes.Contains(uri.Scheme, StringComparer.OrdinalIgnoreCase);
        if (!isAllowedAbsoluteUrl)
        {
            return Result.Failure<LinkTarget>(Error.Validation(
                "LinkTarget.ExternalUrlInvalid", "External URL must be an absolute https, http, mailto: or tel: URL."));
        }

        return Result.Success(new LinkTarget(LinkTargetKind.ExternalUrl, null, null, null, value));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Kind;
        yield return ContentItemId;
        yield return ContentTypeId;
        yield return InternalPath;
        yield return ExternalUrl;
    }
}
