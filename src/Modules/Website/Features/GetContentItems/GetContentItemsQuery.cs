using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentItems;

// CategoryId/TagId/date-range/OrderBy/Direction all default to null so existing positional callers
// (tests, this feature's own first 6 fields) keep compiling unchanged - the task's own requirement
// that today's default order/behavior does not change for a request that omits every new parameter.
public sealed record GetContentItemsQuery(
    Guid? ContentTypeId,
    string? Status,
    string? LanguageCode,
    string? Search,
    bool? IsFeatured,
    Guid? ParentId,
    Guid? CategoryId = null,
    Guid? TagId = null,
    DateTime? PublishedFromUtc = null,
    DateTime? PublishedToUtc = null,
    DateTime? CreatedFromUtc = null,
    DateTime? CreatedToUtc = null,
    string? OrderBy = null,
    string? Direction = null)
    : PagedRequest, IRequest<Result<PagedResult<ContentItemSummaryResponse>>>;
