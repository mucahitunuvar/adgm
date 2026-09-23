using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §6: a flat (non-nested) folder name for organizing the media library. Empty means the
// root - MediaAsset does not require a folder.
public sealed class MediaFolder : ValueObject
{
    public const int MaxLength = 100;

    public string Value { get; }

    private MediaFolder(string value)
    {
        Value = value;
    }

    public static Result<MediaFolder> Create(string? value)
    {
        var trimmed = (value ?? string.Empty).Trim();

        if (trimmed.Length > MaxLength)
        {
            return Result.Failure<MediaFolder>(Error.Validation(
                "MediaFolder.TooLong", $"Folder name must be at most {MaxLength} characters."));
        }

        if (trimmed.Contains('/') || trimmed.Contains('\\'))
        {
            return Result.Failure<MediaFolder>(Error.Validation(
                "MediaFolder.MustBeFlat", "Folder name must not contain '/' or '\\' - nested folders are not supported."));
        }

        return Result.Success(new MediaFolder(trimmed));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
