using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4.1 (Faz 1b Görev 3): tags are per-language, independent of ContentType - the same tag
// concept in two languages is two separate rows with no shared identity (unlike ContentCategory, which
// is one aggregate with per-language translations). (LanguageCode, Slug) uniqueness is an
// Application-layer cross-aggregate check (this type has no repository access to enforce it itself).
public sealed class ContentTag : AggregateRoot
{
    public const int MaxNameLength = 50;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    private ContentTag(Guid id, LanguageCode languageCode, string name, string slug, Guid createdByUserId, DateTime createdAtUtc)
        : base(id)
    {
        LanguageCode = languageCode;
        Name = name;
        Slug = slug;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    private ContentTag()
    {
    }

    public static Result<ContentTag> Create(LanguageCode languageCode, string? name, Guid createdByUserId, DateTime createdAtUtc)
    {
        var nameResult = NormalizeName(name);
        if (nameResult.IsFailure)
        {
            return Result.Failure<ContentTag>(nameResult.Error);
        }

        var slugResult = Domain.Slug.Create(nameResult.Value);
        if (slugResult.IsFailure)
        {
            return Result.Failure<ContentTag>(slugResult.Error);
        }

        return Result.Success(new ContentTag(Guid.NewGuid(), languageCode, nameResult.Value, slugResult.Value.Value, createdByUserId, createdAtUtc));
    }

    public Result Rename(string? name, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var nameResult = NormalizeName(name);
        if (nameResult.IsFailure)
        {
            return nameResult;
        }

        var slugResult = Domain.Slug.Create(nameResult.Value);
        if (slugResult.IsFailure)
        {
            return Result.Failure(slugResult.Error);
        }

        Name = nameResult.Value;
        Slug = slugResult.Value.Value;
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;

        return Result.Success();
    }

    private static Result<string> NormalizeName(string? name)
    {
        var normalized = (name ?? string.Empty).Trim();
        if (normalized.Length == 0 || normalized.Length > MaxNameLength)
        {
            return Result.Failure<string>(Error.Validation(
                "ContentTag.NameInvalid", $"Name is required and must be at most {MaxNameLength} characters."));
        }

        return Result.Success(normalized);
    }
}
