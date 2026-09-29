namespace GenclikMerkezi.Modules.Website.Features.SetContentItemRelatedContent;

public sealed record SetContentItemRelatedContentRequest(byte[] RowVersion, IReadOnlyList<Guid> RelatedContentItemIds);
