using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicContents;

public sealed record PublicContentListResponse(
    string ContentTypeName,
    string ListTemplate,
    PublicContentSeoResponse Seo,
    IReadOnlyList<PublicContentAlternateResponse> Alternates,
    IReadOnlyList<PublicContentCategoryTreeItemResponse> Categories,
    PublicContentCategoryResponse? SelectedCategory,
    PagedResult<PublicContentListItemResponse> Items);
