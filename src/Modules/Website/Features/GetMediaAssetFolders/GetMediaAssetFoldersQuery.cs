using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetMediaAssetFolders;

public sealed record GetMediaAssetFoldersQuery : IRequest<Result<GetMediaAssetFoldersResponse>>;
