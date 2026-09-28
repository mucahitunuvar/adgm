using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetNotFoundPaths;

public sealed record GetNotFoundPathsQuery : PagedRequest, IRequest<Result<PagedResult<NotFoundLogResponse>>>;
