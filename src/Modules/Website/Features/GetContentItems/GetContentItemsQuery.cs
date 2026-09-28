using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentItems;

public sealed record GetContentItemsQuery(
    Guid? ContentTypeId, string? Status, string? LanguageCode, string? Search, bool? IsFeatured, Guid? ParentId)
    : PagedRequest, IRequest<Result<PagedResult<ContentItemSummaryResponse>>>;
