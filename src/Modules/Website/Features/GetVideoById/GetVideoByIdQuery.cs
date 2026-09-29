using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetVideoById;

public sealed record GetVideoByIdQuery(Guid Id) : IRequest<Result<VideoDetailResponse>>;
