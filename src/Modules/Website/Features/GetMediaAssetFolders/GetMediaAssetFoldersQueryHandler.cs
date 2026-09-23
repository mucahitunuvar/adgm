using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetMediaAssetFolders;

public sealed class GetMediaAssetFoldersQueryHandler(IMediaAssetRepository mediaAssetRepository)
    : IRequestHandler<GetMediaAssetFoldersQuery, Result<GetMediaAssetFoldersResponse>>
{
    public async Task<Result<GetMediaAssetFoldersResponse>> Handle(GetMediaAssetFoldersQuery request, CancellationToken cancellationToken)
    {
        var folders = await mediaAssetRepository.GetDistinctFoldersAsync(cancellationToken);
        return Result.Success(new GetMediaAssetFoldersResponse(folders));
    }
}
