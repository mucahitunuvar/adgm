using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetMediaAssetById;

public sealed record GetMediaAssetByIdQuery(Guid Id) : IRequest<Result<MediaAssetDetailResponse>>;
