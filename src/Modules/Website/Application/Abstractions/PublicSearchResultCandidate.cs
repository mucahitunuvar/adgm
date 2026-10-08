namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// Projection of a SearchDocument for the public search endpoint (Görev 3) - deliberately excludes
// NormalizedText and every other internal-only field (it is never returned to a client).
public sealed record PublicSearchResultCandidate(
    string SourceKey,
    string TypeKey,
    string Title,
    string Summary,
    string Url,
    DateTime PublishedAtUtc);
