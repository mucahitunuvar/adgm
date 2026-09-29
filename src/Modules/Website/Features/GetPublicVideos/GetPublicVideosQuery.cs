using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicVideos;

public sealed record GetPublicVideosQuery(string? Lang) : PagedRequest, IRequest<Result<PagedResult<PublicVideoResponse>>>;
