namespace GenclikMerkezi.Modules.Website.Features.GetPublicSearch;

public sealed record PublicSearchResultItemResponse(
    string SourceKey,
    string TypeKey,
    string Title,
    string Summary,
    string Url,
    DateTime PublishedAtUtc);
