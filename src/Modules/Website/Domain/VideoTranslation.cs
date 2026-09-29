using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §5 (Faz 1b Görev 2). Mirrors ContentTypeTranslation's shape: Update is internal (only Video,
// the aggregate root, calls it), Create/Update share the same normalization so both paths enforce the
// same invariants.
public sealed class VideoTranslation : Entity
{
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 500;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    private VideoTranslation(Guid id, LanguageCode languageCode, string title, string? description)
        : base(id)
    {
        LanguageCode = languageCode;
        Title = title;
        Description = description;
    }

    private VideoTranslation()
    {
    }

    public static Result<VideoTranslation> Create(LanguageCode languageCode, string? title, string? description)
    {
        var titleResult = NormalizeTitle(title);
        if (titleResult.IsFailure)
        {
            return Result.Failure<VideoTranslation>(titleResult.Error);
        }

        var descriptionResult = NormalizeDescription(description);
        if (descriptionResult.IsFailure)
        {
            return Result.Failure<VideoTranslation>(descriptionResult.Error);
        }

        return Result.Success(new VideoTranslation(Guid.NewGuid(), languageCode, titleResult.Value, descriptionResult.Value));
    }

    internal Result Update(string? title, string? description)
    {
        var titleResult = NormalizeTitle(title);
        if (titleResult.IsFailure)
        {
            return titleResult;
        }

        var descriptionResult = NormalizeDescription(description);
        if (descriptionResult.IsFailure)
        {
            return descriptionResult;
        }

        Title = titleResult.Value;
        Description = descriptionResult.Value;

        return Result.Success();
    }

    private static Result<string> NormalizeTitle(string? title)
    {
        var normalized = (title ?? string.Empty).Trim();
        if (normalized.Length == 0 || normalized.Length > MaxTitleLength)
        {
            return Result.Failure<string>(Error.Validation(
                "VideoTranslation.TitleInvalid", $"Title is required and must be at most {MaxTitleLength} characters."));
        }

        return Result.Success(normalized);
    }

    private static Result<string?> NormalizeDescription(string? description)
    {
        var trimmed = description?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return Result.Success<string?>(null);
        }

        if (trimmed.Length > MaxDescriptionLength)
        {
            return Result.Failure<string?>(Error.Validation(
                "VideoTranslation.DescriptionInvalid", $"Description must be at most {MaxDescriptionLength} characters."));
        }

        return Result.Success<string?>(trimmed);
    }
}
