namespace GenclikMerkezi.Modules.Website.Features.GetContentItemRevisionByNumber;

public sealed record ContentItemRevisionDetailResponse(
    int RevisionNumber,
    DateTime SavedAtUtc,
    Guid SavedByUserId,
    string Kind,
    IReadOnlyList<string> ChangedLanguages,
    bool IsPublishedSnapshot,
    IReadOnlyList<ContentItemRevisionTranslationSnapshotResponse> Translations,
    IReadOnlyList<Guid> CategoryIds);
