using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeleteMediaAsset;

public sealed record DeleteMediaAssetCommand(Guid Id) : IRequest<Result>;
