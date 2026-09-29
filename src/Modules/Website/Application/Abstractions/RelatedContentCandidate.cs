namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §4.1 (Faz 1b Görev 5): RelatedContentResolutionService's projection shape - only the
// requested language's display fields, never the full translation collection or aggregate (the
// master prompt's "projection kullanır, tam aggregate yüklenmez" requirement).
public sealed record RelatedContentCandidate(
    Guid Id, string Title, string Summary, string FullPath, Guid? CoverImageMediaId, DateTime EffectivePublishDate);
