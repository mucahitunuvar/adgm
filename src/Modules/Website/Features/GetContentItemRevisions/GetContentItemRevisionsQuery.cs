using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentItemRevisions;

public sealed record GetContentItemRevisionsQuery(Guid ContentItemId) : PagedRequest, IRequest<Result<PagedResult<ContentItemRevisionSummaryResponse>>>;
