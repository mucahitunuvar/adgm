using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §5 (Faz 1b Görev 2). RowVersion is the same application-managed optimistic-concurrency
// token ContentType/SiteSettings already use (Görev 4's cover-image-in-use check and every other
// cross-aggregate invariant this type cannot check by itself belong to the Application layer, the same
// separation ContentType's own remarks describe).
public sealed class Video : AggregateRoot
{
    private readonly List<VideoTranslation> _translations = [];

    public YouTubeVideoId YouTubeVideoId { get; private set; } = null!;

    public Guid? CoverImageMediaId { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyList<VideoTranslation> Translations => _translations.AsReadOnly();

    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    private Video(
        Guid id, YouTubeVideoId youTubeVideoId, Guid? coverImageMediaId, int sortOrder, Guid createdByUserId, DateTime createdAtUtc)
        : base(id)
    {
        YouTubeVideoId = youTubeVideoId;
        CoverImageMediaId = coverImageMediaId;
        SortOrder = sortOrder;
        IsActive = true;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    private Video()
    {
    }

    public static Result<Video> Create(
        string? youTubeUrl,
        Guid? coverImageMediaId,
        int sortOrder,
        LanguageCode defaultLanguageCode,
        string? defaultLanguageTitle,
        string? defaultLanguageDescription,
        Guid createdByUserId,
        DateTime createdAtUtc)
    {
        var youTubeVideoIdResult = YouTubeVideoId.Create(youTubeUrl);
        if (youTubeVideoIdResult.IsFailure)
        {
            return Result.Failure<Video>(youTubeVideoIdResult.Error);
        }

        var translationResult = VideoTranslation.Create(defaultLanguageCode, defaultLanguageTitle, defaultLanguageDescription);
        if (translationResult.IsFailure)
        {
            return Result.Failure<Video>(translationResult.Error);
        }

        var video = new Video(Guid.NewGuid(), youTubeVideoIdResult.Value, coverImageMediaId, sortOrder, createdByUserId, createdAtUtc);
        video._translations.Add(translationResult.Value);

        return Result.Success(video);
    }

    public Result Update(string? youTubeUrl, Guid? coverImageMediaId, int sortOrder, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var youTubeVideoIdResult = YouTubeVideoId.Create(youTubeUrl);
        if (youTubeVideoIdResult.IsFailure)
        {
            return youTubeVideoIdResult;
        }

        YouTubeVideoId = youTubeVideoIdResult.Value;
        CoverImageMediaId = coverImageMediaId;
        SortOrder = sortOrder;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // Upserts the translation for languageCode - one call per language the caller wants to set.
    public Result SetTranslation(LanguageCode languageCode, string? title, string? description, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is not null)
        {
            var updateResult = existing.Update(title, description);
            if (updateResult.IsFailure)
            {
                return updateResult;
            }

            Touch(updatedByUserId, updatedAtUtc);
            return Result.Success();
        }

        var createResult = VideoTranslation.Create(languageCode, title, description);
        if (createResult.IsFailure)
        {
            return createResult;
        }

        _translations.Add(createResult.Value);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // The default-language translation can never be removed (Application layer resolves which
    // language is default and passes it in - Video itself has no SiteLanguage access, mirroring
    // ContentType.RemoveTranslation).
    public Result RemoveTranslation(LanguageCode languageCode, LanguageCode defaultLanguageCode, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (languageCode == defaultLanguageCode)
        {
            return Result.Failure(Error.Conflict(
                "Video.CannotDeleteDefaultTranslation", "The default language's translation cannot be deleted."));
        }

        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is null)
        {
            return Result.Failure(Error.NotFound("Video.TranslationNotFound", $"No translation exists for language '{languageCode}'."));
        }

        _translations.Remove(existing);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    public Result Activate(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        IsActive = true;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    public Result Deactivate(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        IsActive = false;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    private void Touch(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;
        RowVersion = Guid.NewGuid().ToByteArray();
    }
}
