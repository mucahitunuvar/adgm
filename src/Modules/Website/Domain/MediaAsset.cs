using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §6. Invariant: ContainsPersonalData = true requires a non-empty UsagePermissionNote -
// checked both on Create and on UpdateMetadata (the flag can be turned on later).
public sealed class MediaAsset : AggregateRoot
{
    private readonly List<MediaAssetVariant> _variants = [];
    private readonly List<MediaAssetTranslation> _translations = [];

    public MediaAssetKind Kind { get; private set; }

    public FileAttachment Original { get; private set; } = null!;

    public IReadOnlyList<MediaAssetVariant> Variants => _variants.AsReadOnly();

    public IReadOnlyList<MediaAssetTranslation> Translations => _translations.AsReadOnly();

    // Only set for Kind == Image (the original's own pixel dimensions, before any variant resizing).
    public int? Width { get; private set; }

    public int? Height { get; private set; }

    public MediaFolder Folder { get; private set; } = null!;

    public string Source { get; private set; } = string.Empty;

    public string UsagePermissionNote { get; private set; } = string.Empty;

    public bool ContainsPersonalData { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    private MediaAsset(
        Guid id,
        MediaAssetKind kind,
        FileAttachment original,
        IEnumerable<MediaAssetVariant> variants,
        int? width,
        int? height,
        MediaFolder folder,
        string source,
        string usagePermissionNote,
        bool containsPersonalData,
        Guid createdByUserId,
        DateTime createdAtUtc)
        : base(id)
    {
        Kind = kind;
        Original = original;
        _variants.AddRange(variants);
        Width = width;
        Height = height;
        Folder = folder;
        Source = source;
        UsagePermissionNote = usagePermissionNote;
        ContainsPersonalData = containsPersonalData;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    private MediaAsset()
    {
    }

    // Takes an explicit id (unlike most other aggregates' Create, which self-generate one) because
    // the caller must upload the original file and its variants - tagging each FileAttachment's
    // OwnerEntityId with this same MediaAsset id - before this aggregate can be constructed.
    public static Result<MediaAsset> Create(
        Guid id,
        MediaAssetKind kind,
        FileAttachment original,
        IReadOnlyList<MediaAssetVariant> variants,
        int? width,
        int? height,
        MediaFolder folder,
        string? source,
        string? usagePermissionNote,
        bool containsPersonalData,
        Guid createdByUserId,
        DateTime createdAtUtc)
    {
        var normalizedSource = (source ?? string.Empty).Trim();
        var normalizedPermissionNote = (usagePermissionNote ?? string.Empty).Trim();

        var invariantResult = CheckPersonalDataInvariant(containsPersonalData, normalizedPermissionNote);
        if (invariantResult.IsFailure)
        {
            return Result.Failure<MediaAsset>(invariantResult.Error);
        }

        return Result.Success(new MediaAsset(
            id, kind, original, variants, width, height, folder,
            normalizedSource, normalizedPermissionNote, containsPersonalData, createdByUserId, createdAtUtc));
    }

    public Result UpdateMetadata(
        MediaFolder folder,
        string? source,
        string? usagePermissionNote,
        bool containsPersonalData,
        Guid updatedByUserId,
        DateTime updatedAtUtc)
    {
        var normalizedSource = (source ?? string.Empty).Trim();
        var normalizedPermissionNote = (usagePermissionNote ?? string.Empty).Trim();

        var invariantResult = CheckPersonalDataInvariant(containsPersonalData, normalizedPermissionNote);
        if (invariantResult.IsFailure)
        {
            return invariantResult;
        }

        Folder = folder;
        Source = normalizedSource;
        UsagePermissionNote = normalizedPermissionNote;
        ContainsPersonalData = containsPersonalData;
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;

        return Result.Success();
    }

    // Upserts the translation for languageCode - one call per language the caller wants to set.
    public void SetTranslation(LanguageCode languageCode, string? altText, string? caption, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is not null)
        {
            existing.Update(altText, caption);
        }
        else
        {
            _translations.Add(MediaAssetTranslation.Create(languageCode, altText, caption));
        }

        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;
    }

    private static Result CheckPersonalDataInvariant(bool containsPersonalData, string normalizedPermissionNote)
    {
        if (containsPersonalData && normalizedPermissionNote.Length == 0)
        {
            return Result.Failure(Error.Validation(
                "MediaAsset.UsagePermissionNoteRequired",
                "A usage permission note is required when the media asset contains personal data."));
        }

        return Result.Success();
    }
}
