using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public sealed record ContentItemRevisionSummary(
    Guid Id,
    int RevisionNumber,
    DateTime SavedAtUtc,
    Guid SavedByUserId,
    ContentItemRevisionKind Kind,
    IReadOnlyList<string> ChangedLanguages,
    bool IsPublishedSnapshot);
