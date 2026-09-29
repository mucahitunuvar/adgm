using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetVideos;

public sealed record GetVideosQuery(bool? IsActive, string? Search) : PagedRequest, IRequest<Result<PagedResult<VideoSummaryResponse>>>;
