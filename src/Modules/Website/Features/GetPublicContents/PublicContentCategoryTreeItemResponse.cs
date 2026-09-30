namespace GenclikMerkezi.Modules.Website.Features.GetPublicContents;

public sealed record PublicContentCategoryTreeItemResponse(
    Guid Id, string Name, string Slug, IReadOnlyList<PublicContentCategoryTreeItemResponse> Children);
